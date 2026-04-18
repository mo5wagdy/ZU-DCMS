using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Booking.Commands.CreateBooking
{
    public class CreateBookingHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ISessionService _sessionService;
        private readonly IPatientService _patientService;
        private readonly IPaymentService _paymentService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserCodeGenerator _codeGen;
        private readonly IMapper _mapper;
        private readonly IAppLogger<CreateBookingHandler> _logger;

        public CreateBookingHandler(
            IUnitOfWork uow,
            ISessionService sessionService,
            IPatientService patientService,
            IPaymentService paymentService,
            IEventPublisher eventPublisher,
            IUserCodeGenerator codeGen,
            IMapper mapper,
            IAppLogger<CreateBookingHandler> logger)
        {
            _uow = uow;
            _sessionService = sessionService;
            _patientService = patientService;
            _paymentService = paymentService;
            _eventPublisher = eventPublisher;
            _codeGen = codeGen;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<BookingDto>> Handle(CreateBookingCommand command)
        {
            var dto = command.Dto;
            var patientId = command.PatientId;

            _logger.LogInfo("Creating booking for PatientId: {PatientId} with PreferredDate: {PreferredDate} and TimeSlot: {TimeSlot}", patientId, dto.PreferredDate, dto.PreferredTimeSlot);

            var patient = await _patientService.GetByIdAsync(patientId);

            if (patient.IsFailure)
            {
                _logger.LogWarning("Patient not found for PatientId: {PatientId}", patientId);
                return Result.Failure<BookingDto>("المريض غير موجود");
            }

            var hasActiveBooking = await _uow.Repository<Domain.Entities.Booking>().ExistsAsync
                (b => b.PatientId == patientId &&
                      b.Status != BookingStatus.Cancelled &&
                      b.Status != BookingStatus.Completed
                );

            if (hasActiveBooking)
            {
                _logger.LogWarning("Patient {PatientId} already has an active booking", patientId);
                return Result.Failure<BookingDto>("لديك حجز نشط بالفعل");
            }

            var sessionResult = await _sessionService.FindSessionAsync(dto.PreferredDate, dto.PreferredTimeSlot);

            if (sessionResult.IsFailure)
            {
                _logger.LogWarning("Session not found for PreferredDate: {PreferredDate} and TimeSlot: {TimeSlot}", dto.PreferredDate, dto.PreferredTimeSlot);
                return Result.Failure<BookingDto>(sessionResult.Error);
            }

            var session = sessionResult.Value;

            var feeResult = await GetDiagnosisFeeAsync();

            if (feeResult.IsFailure)
            {
                _logger.LogError("Failed to retrieve diagnosis fee");
                return Result.Failure<BookingDto>(feeResult.Error);
            }

            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Reserving slot for SessionId: {SessionId} and BookingType: {BookingType}", session.Id, dto.BookingType);

                var reserve = await _sessionService.ReserveSlotAsync(session.Id, dto.BookingType);

                if (reserve.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    _logger.LogError("Failed to reserve slot for Session");
                    return Result.Failure<BookingDto>(reserve.Error);
                }

                var booking = new Domain.Entities.Booking
                {
                    BookingCode = await _codeGen.GenerateAsync("BKN", "BookingCodeSeq"),
                    PatientId = patientId,
                    SessionId = session.Id,
                    BookingType = dto.BookingType,
                    Status = BookingStatus.Pending,
                    PreliminaryComplaint = dto.PreliminaryComplaint?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Repository<Domain.Entities.Booking>().AddAsync(booking);

                var payment = _paymentService.CreateDiagnosisPaymentAsync(patientId, booking.Id, feeResult.Value).Result;

                if (payment.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    _logger.LogError("Failed to create payment record for BookingId");
                    return Result.Failure<BookingDto>(payment.Error);
                }

                await _uow.SaveChangesAsync();

                var paymentCode = _paymentService.GeneratePaymentCodeAsync(payment.Value).Result;

                if (paymentCode.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    _logger.LogError("Failed to generate payment code for BookingId");
                    return Result.Failure<BookingDto>(paymentCode.Error);
                }

                await _uow.CommitTransactionAsync(patientId.ToString());

                _logger.LogInfo("Booking created {BookingId}", booking.Id);

                await _eventPublisher.PublishAsync(new BookingCreatedEvent(booking.Id, booking.SessionId));

                var full = await _uow.Repository<Domain.Entities.Booking>().GetFirstOrDefaultAsync
                 (
                    b => b.Id == booking.Id,
                    false,
                    b => b.Patient,
                    b => b.Session,
                    b => b.Payment
                 );

                return Result.Success(_mapper.Map<BookingDto>(full!));
            }

            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("Error creating booking", ex);
                return Result.Failure<BookingDto>("حدث خطأ أثناء الحجز");
            }
        }

        private async Task<Result<decimal>> GetDiagnosisFeeAsync()
        {
            var config = await _uow.Repository<SystemConfig>().GetFirstOrDefaultAsync(c => c.Key == ConfigKeys.DiagnosisFee);

            if (config == null)
                return Result.Failure<decimal>("إعداد سعر الكشف غير موجود");

            return decimal.TryParse(config.Value, out var fee) ? Result.Success(fee) : Result.Failure<decimal>("إعداد سعر الكشف غير صحيح");
        }
    }
}
