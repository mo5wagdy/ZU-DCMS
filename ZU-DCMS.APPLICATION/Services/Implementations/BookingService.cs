using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;
        private readonly ISessionService _sessionService;
        private readonly IPatientService _patientService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notification;
        private readonly IUserCodeGenerator _codeGen;
        private readonly ISignalRService _signalR;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<BookingService> _logger;

        public BookingService(
            IUnitOfWork uow,
            ISessionService sessionService,
            IPatientService patientService,
            IPaymentService paymentService,
            IPaymentGateway paymentGateway,
            INotificationService notification,
            IUserCodeGenerator codeGen,
            ISignalRService signalR,
            ICacheService cache,
            IMapper mapper,
            IAppLogger<BookingService> logger)
        {
            _uow = uow;
            _sessionService = sessionService;
            _patientService = patientService;
            _paymentGateway = paymentGateway;
            _paymentService = paymentService;
            _notification = notification;
            _codeGen = codeGen;
            _signalR = signalR;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Get patient bookings with pagination __ //
        public async Task<Result<PagedResult<BookingDto>>> GetPatientBookingsAsync(int patientId, PagedRequest request)
        {
            _logger.LogInfo("Fetching bookings for patient {PatientId} - Page: {Page}, PageSize: {PageSize}", patientId, request.Page, request.PageSize);

            // __ Get total count and paged items in one query __ //
            var (items, total) = await _uow.Repository<Booking>().GetPagedListAsync
                (
                    (request.Page - 1) * request.PageSize,
                    request.PageSize,
                    b => b.PatientId == patientId,
                    true,
                    q => q.OrderByDescending(b => b.CreatedAt),
                    b => b.Session,
                    b => b.Payment
                );

            // __ Map to DTOs __ //
            var dtos = _mapper.Map<List<BookingDto>>(items);

            // __ Return paged result __ // 
            var pagedResult = PagedResult<BookingDto>.Create(dtos, total, request);

            _logger.LogInfo("Fetched {Count} bookings for patient {PatientId}", dtos.Count, patientId);

            return Result.Success<PagedResult<BookingDto>>(pagedResult);
        }

        // __ Get booking by ID with full details __ //
        public async Task<Result<BookingDto>> GetByIdAsync(int bookingId)
        {
            _logger.LogInfo("Fetching booking details for BookingId: {BookingId}", bookingId);

            // __ Load booking with related entities __ //
            var booking = await _uow.Repository<Booking>().GetFirstOrDefaultAsync
            (
                b => b.Id == bookingId,
                false,
                b => b.Patient,
                b => b.Session,
                b => b.Payment
            );

            // __ Check if booking exists __ //
            if (booking is null)
            {
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", bookingId);

                return Result.Failure<BookingDto>("الحجز غير موجود");
            }

            _logger.LogInfo("Booking details fetched for BookingId: {BookingId}", bookingId);

            // __ Map to DTO and return __ //
            return Result.Success(_mapper.Map<BookingDto>(booking));
        }

        // __ Create a new booking __ //
        public async Task<Result<BookingDto>> CreateBookingAsync(int patientId, CreateBookingDto dto)
        {
            _logger.LogInfo("Creating booking for PatientId: {PatientId} with PreferredDate: {PreferredDate} and TimeSlot: {TimeSlot}", patientId, dto.PreferredDate, dto.PreferredTimeSlot);

            // __ Verify patient exists __ //
            var patient = await _patientService.GetByIdAsync(patientId);

            // __ If patient not found, return failure __ //
            if (patient.IsFailure)
            {
                _logger.LogWarning("Patient not found for PatientId: {PatientId}", patientId);

                return Result.Failure<BookingDto>("المريض غير موجود");
            }

            // __ Prevent duplicate active bookings per patient __ //
            var hasActiveBooking = await _uow.Repository<Booking>().ExistsAsync
                (b => b.PatientId == patientId &&
                      b.Status != BookingStatus.Cancelled &&
                      b.Status != BookingStatus.Completed
                );

            // __ If active booking exists, return failure __ //
            if (hasActiveBooking)
            {
                _logger.LogWarning("Patient {PatientId} already has an active booking", patientId);

                return Result.Failure<BookingDto>("لديك حجز نشط بالفعل");
            }

            // __ Find session for preferred date and time slot __ //
            var sessionResult = await _sessionService.FindSessionAsync(dto.PreferredDate, dto.PreferredTimeSlot);

            // __ If no session found for the preferred date and time slot, return failure __ //
            if (sessionResult.IsFailure)
            {
                _logger.LogWarning("Session not found for PreferredDate: {PreferredDate} and TimeSlot: {TimeSlot}", dto.PreferredDate, dto.PreferredTimeSlot);

                return Result.Failure<BookingDto>(sessionResult.Error);
            }

            // __ Session found, proceed with booking creation __ //
            var session = sessionResult.Value;

            // __ Get diagnosis fee from system config __ //
            var feeResult = await GetDiagnosisFeeAsync();

            // __ If failed to get fee, return failure __ //
            if (feeResult.IsFailure)
            {
                _logger.LogError("Failed to retrieve diagnosis fee");

                return Result.Failure<BookingDto>(feeResult.Error);
            }

            // __ Begin transaction for booking creation __ //
            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Reserving slot for SessionId: {SessionId} and BookingType: {BookingType}", session.Id, dto.BookingType);

                // __ Reserve a slot in the session __ //
                var reserve = await _sessionService.ReserveSlotAsync(session.Id, dto.BookingType);

                // __ If failed to reserve slot, rollback transaction and return failure __ //
                if (reserve.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    
                    _logger.LogError("Failed to reserve slot for Session");
                    
                    return Result.Failure<BookingDto>(reserve.Error);
                }

                // __ Create booking entity __ //
                var booking = new Booking
                {
                    BookingCode = await _codeGen.GenerateAsync("BKN", "BookingCodeSeq"),
                    PatientId = patientId,
                    SessionId = session.Id,
                    BookingType = dto.BookingType,
                    Status = BookingStatus.Pending,
                    PreliminaryComplaint = dto.PreliminaryComplaint?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                // __ Add booking to database __ //
                await _uow.Repository<Booking>().AddAsync(booking);

                // __ Create payment record __ //
                var payment = new Payment
                {
                    PatientId = patientId,
                    Booking = booking,
                    Amount = feeResult.Value,
                    Type = PaymentType.Diagnosis,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                // __ Add payment to database __ //
                await _uow.Repository<Payment>().AddAsync(payment);

                // __ Save changes to get generated IDs for booking and payment __ //
                await _uow.SaveChangesAsync();

                // Generate Fawry payment code
                var paymentResult = await _paymentGateway.GeneratePaymentCodeAsync(payment.Id, payment.Amount);

                // __ If failed to generate payment code, rollback transaction and return failure __ //
                if (!paymentResult.IsSuccess)
                {

                    await _uow.RollbackTransactionAsync();
                    
                    _logger.LogError("Failed to generate payment code for PaymentId");

                    return Result.Failure<BookingDto>("حدث خطأ في توليد كود الدفع، حاول مرة أخرى");
                }


                // __ Update payment with gateway response __ //
                payment.PaymentCode = paymentResult.PaymentCode;
                payment.GatewayName = paymentResult.GatewayName;
                payment.GatewayReference = paymentResult.GatewayReference;

                // __ Update payment entity __ //
                _uow.Repository<Payment>().Update(payment);

                // __ Single SaveChanges via CommitTransaction __ //
                await _uow.CommitTransactionAsync(patientId.ToString());

                _logger.LogInfo("Booking created {BookingId}", booking.Id);

                // __ Handle post-booking side effects (cache invalidation, notifications, SignalR) __ //
                await HandleBookingSideEffectsAsync(booking, () => _notification.SendBookingConfirmedAsync(booking.Id), SignalREvents.BookingCreated);

                // __ Load full booking details to return in response __ //
                var full = await LoadFullBookingAsync(booking.Id);
                return Result.Success(_mapper.Map<BookingDto>(full!));
            }

            // __ Catch any exceptions, log error, rollback transaction, and return failure __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("Error creating booking", ex);

                return Result.Failure<BookingDto>("حدث خطأ أثناء الحجز");
            }
        }


        // __ Cancel an existing booking __ //
        public async Task<Result> CancelBookingAsync(int bookingId, int patientId)
        {
            _logger.LogInfo("Cancelling booking {BookingId} for PatientId: {PatientId}", bookingId, patientId);

            // __ Load booking with related entities __ //
            var booking = await LoadFullBookingAsync(bookingId);

            // __ Check if booking exists __ //
            if (booking is null)
            { 
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", bookingId);

                return Result.Failure("الحجز غير موجود");
            }

            // __ Check if the booking belongs to the patient __ //
            if (booking.PatientId != patientId)
            {
                _logger.LogWarning("Unauthorized cancellation attempt for BookingId: {BookingId} by PatientId: {PatientId}", bookingId, patientId);
                
                return Result.Failure("غير مصرح");
            }

            // __ If booking paid, cannot cancel __ //
            if (booking.Payment?.Status == PaymentStatus.Paid)
            {
                _logger.LogWarning("Attempt to cancel paid booking {BookingId}", bookingId);

                return Result.Failure("لا يمكن إلغاء الحجز بعد الدفع");
            }

            // __ Transaction for cancellation process __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Update booking status to Cancelled __ //
                booking.Status = BookingStatus.Cancelled;
                _uow.Repository<Booking>().Update(booking);

                // __ Release the reserved slot in the session __ //
                var release = await _sessionService.ReleaseSlotAsync(booking.SessionId, booking.BookingType);

                // __ If failed to release slot, rollback transaction and return failure __ //
                if (release.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();

                    _logger.LogError("Failed to release slot during cancellation of BookingId");

                    return Result.Failure(release.Error);
                }

                // __ Save changes to database __ //
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Booking cancelled {BookingId}", bookingId);

                // __ Handle post-cancellation side effects (cache invalidation, notifications, SignalR) __ //
                await HandleBookingSideEffectsAsync(booking, () => _notification.SendBookingCancelledAsync(booking.Id), SignalREvents.BookingCancelled);

                return Result.Success();
            }

            // __ Catch any exceptions, log error, rollback transaction, and return failure __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("Error cancelling booking", ex);
                
                return Result.Failure("فشل الإلغاء");
            }
        }


        // __ Postpone an existing booking to a new session __ //
        public async Task<Result> PostponeBookingAsync(int bookingId, string reason, string adminId)
        {
            _logger.LogInfo("Postponing booking {BookingId} by AdminId: {AdminId} with reason: {Reason}", bookingId, adminId, reason);

            // __ Load booking with related entities __ //
            var booking = await LoadFullBookingAsync(bookingId);

            // __ Check if booking exists __ //
            if (booking is null)
            {
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", bookingId);

                return Result.Failure("الحجز غير موجود");
            }

            // __ Get available slots for the booking type __ //
            var slots = await _sessionService.GetAvailableSlotsAsync(booking.BookingType);

            // __ If no available slots, return failure __ //
            if (slots.IsFailure || slots.Value.Count == 0)
            {
                _logger.LogWarning("No available slots found for BookingType: {BookingType} during postponement of BookingId", booking.BookingType);

                return Result.Failure("لا يوجد مواعيد");
            }

            // __ Get the new session ID from the first available slot __ //
            var newSessionId = slots.Value.First().SessionId;

            // __ Transaction for postponement process __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Release the currently reserved slot in the old session __ //
                var release = await _sessionService.ReleaseSlotAsync(booking.SessionId, booking.BookingType);

                // __ If failed to release old slot, rollback transaction and return failure __ //
                if (release.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    
                    _logger.LogError("Failed to release old slots");
                    
                    return Result.Failure(release.Error);
                }

                // __ Reserve a slot in the new session __ //
                var reserve = await _sessionService.ReserveSlotAsync(newSessionId, booking.BookingType);

                // __ If failed to reserve new slot, rollback transaction and return failure __ //
                if (reserve.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    
                    _logger.LogError("Failed to reserve new slot for Session during postponement");    
                    
                    return Result.Failure(reserve.Error);
                }

                // __ Update booking with new session and postponement details __ //
                booking.SessionId = newSessionId;
                booking.Status = BookingStatus.Postponed;
                booking.PostponeReason = reason.Trim();
                booking.PostponedAt = DateTime.UtcNow;

                // __ Update booking entity __ //
                _uow.Repository<Booking>().Update(booking);

                // __ Save changes to database __ //
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Booking postponed {BookingId}", bookingId);

                // __ Handle post-postponement side effects (cache invalidation, notifications, SignalR) __ //
                await HandleBookingSideEffectsAsync(booking, () => _notification.SendBookingPostponedAsync(booking.Id, reason), SignalREvents.BookingPostponed);

                return Result.Success();
            }

            // __ Catch any exceptions, log error, rollback transaction, and return failure __ //
            catch (Exception ex)
            {
                _logger.LogError("Error postponing booking", ex);
                
                await _uow.RollbackTransactionAsync();
                
                return Result.Failure("فشل التأجيل");
            }
        }


        // __ Helper Methods __ //

        // __ Helper method to load booking with related entities __ //
        private async Task<Booking?> LoadFullBookingAsync(int id) => await _uow.Repository<Booking>()
            .GetFirstOrDefaultAsync
            (b => b.Id == id,
                  false,
                  b => b.Patient,
                  b => b.Session,
                  b => b.Payment
            );

        // __ Helper method to handle side effects after booking operations __ //
        private async Task HandleBookingSideEffectsAsync(Booking booking, Func<Task> notificationAction, string signalREvent)
        {
            // 1. Cache
            await _cache.RemoveAsync(CacheKeys.SessionStatus(booking.SessionId));

            // 2. Notification
            await TrySafe(notificationAction, $"Notification - BookingId: {booking.Id}");

            // 3. SignalR
            await TrySafe(async () => await _signalR.SendDashboardUpdateAsync(signalREvent, new { bookingId = booking.Id, sessionId = booking.SessionId }), $"SignalR - {signalREvent} - BookingId: {booking.Id}");
        }

        // __ Helper method to get the diagnosis fee from system configuration __ //
        private async Task<Result<decimal>> GetDiagnosisFeeAsync()
        {
            var config = await _uow.Repository<SystemConfig>().GetFirstOrDefaultAsync(c => c.Key == ConfigKeys.DiagnosisFee);

            if (config == null)
                return Result.Failure<decimal>("إعداد سعر الكشف غير موجود");

            return decimal.TryParse(config.Value, out var fee) ? Result.Success(fee) : Result.Failure<decimal>("إعداد سعر الكشف غير صحيح");
        }

        // __ Helper method to execute side effects safely without affecting main flow __ //
        private async Task TrySafe(Func<Task> action, string context)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError("Side effect failed: {Context}", ex, context);
            }
        }
    }
}
