using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRawSqlExecutor _sql;
        private readonly IUserCodeGenerator _codeGen;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<CreateBookingHandler> _logger;

        public CreateBookingHandler(
            IUnitOfWork uow,
            IRawSqlExecutor sql,
            IUserCodeGenerator codeGen,
            IFusionCache cache,
            IMapper mapper,
            IAppLogger<CreateBookingHandler> logger)
        {
            _uow = uow;
            _sql = sql;
            _codeGen = codeGen;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        // __ Create a new booking __ //
        public async Task<Result<BookingDto>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;
            
            var patientId = command.PatientId;

            _logger.LogInfo("Creating booking for PatientId: {PatientId} with PreferredDate: {PreferredDate} and TimeSlot: {TimeSlot}", patientId, dto.PreferredDate, dto.PreferredTimeSlot);

            // __ Verify patient exists __ //
            var patient = await _uow.Repository<Patient>().GetFirstOrDefaultAsync(p => p.Id == patientId);

            // __ If patient not found, return failure __ //
            if (patient is null)
            {
                _logger.LogWarning("Patient not found for PatientId: {PatientId}", patientId);
                
                return Result.Failure<BookingDto>("Patient not found");
            }

            // __ Prevent duplicate active bookings per patient (FollowUp has its own validation below) __ //
            var hasActiveBooking = dto.BookingType != BookingType.FollowUp &&
                await _uow.Repository<Booking>().ExistsAsync
                (b =>
                    b.PatientId == patientId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Completed
                );

            // __ If active booking exists, return failure __ //
            if (hasActiveBooking)
            {
                _logger.LogWarning("Patient {PatientId} already has an active booking", patientId);
                
                return Result.Failure<BookingDto>("You already have an active booking");
            }

            // __ Prevent New booking if patient has an active case __ //
            if (dto.BookingType == BookingType.New)
            {
                var hasActiveCase = await _uow.Repository<CaseAssignment>().ExistsAsync
                (
                    c => c.DiagnosisRecord.Booking.PatientId == patientId &&
                         c.Status != CaseStatus.Completed &&
                         c.Status != CaseStatus.Approved &&
                         c.Status != CaseStatus.Transferred
                );
                         
                if (hasActiveCase)
                {
                    return Result.Failure<BookingDto>("You have a case in progress, you must book a follow-up appointment instead of a new one");
                }
            }

            int? followUpClinicId = null;

            CaseAssignment? lastCase = null;
            CaseSession? lastSession = null;

            if (dto.BookingType == BookingType.FollowUp)
            {
                // Find last active CaseAssignment for this patient
                lastCase = await _uow.Repository<CaseAssignment>().GetFirstOrDefaultAsync
                    (c => 
                            c.DiagnosisRecord.Booking.PatientId == patientId &&
                            c.Status != CaseStatus.Completed &&
                            c.Status != CaseStatus.Approved &&
                            c.Status != CaseStatus.Transferred, 
                            includes: c => c.Sessions
                    );

                if (lastCase is null)
                    return Result.Failure<BookingDto>("Cannot book a follow-up without a previous treatment case");

                lastSession = lastCase.Sessions
                    .OrderByDescending(s => s.SessionDate)
                    .FirstOrDefault();

                if (lastSession is null || !lastSession.HasFollowUp)
                    return Result.Failure<BookingDto>("No follow-up required for your current case");

                followUpClinicId = lastCase.ClinicId;
            }

            // __ Validate time slot format __ //
            if (!TimeSpan.TryParse(dto.PreferredTimeSlot, out var time))
                return Result.Failure<BookingDto>("Invalid time format");

            // __ Find session for preferred date and time slot __ //
            var sessionResult = await _uow.Repository<Session>().GetFirstOrDefaultAsync
                (s => 
                    s.Date.Date == dto.PreferredDate.Date &&
                    s.StartTime == time &&
                    s.IsActive &&
                   !s.IsDeleted
                );

            // __ If no session found for the preferred date and time slot, or it has ended today __ //
            if (sessionResult is null || (sessionResult.Date.Date == DateTime.Today && sessionResult.EndTime <= DateTime.Now.TimeOfDay))
            {
                _logger.LogWarning("Session not found or ended for PreferredDate: {PreferredDate} and TimeSlot: {TimeSlot}", dto.PreferredDate, dto.PreferredTimeSlot);
                
                return Result.Failure<BookingDto>("Appointment not available or time has passed");
            }

            // __ Session found, proceed with booking creation __ //
            var session = sessionResult;

            // __ Begin transaction for booking creation __ //
            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Reserving slot for SessionId: {SessionId} and BookingType: {BookingType}", session.Id, dto.BookingType);

                // __ Reserve a slot in the session __ //
                _logger.LogInfo("Reserving slot for SessionId: {SessionId}, Type: {Type}", session.Id, dto.BookingType);

                // __ We will use a raw SQL update to increment the count atomically and ensure we do not exceed the max limits __ //

                // __ Determine which column to update based on booking Type __ //
                var column = dto.BookingType == BookingType.New ? nameof(Session.CurrentNewCount) : nameof(Session.CurrentFollowUpCount);

                // __ Determine the max column for the check __ //
                var maxColumn = dto.BookingType == BookingType.New ? nameof(Session.MaxNewPatients) : nameof(Session.MaxFollowUpPatients);

                // __ Construct the SQL query to increment the count if it does not exceed the max and the session is active and not deleted __ //
                var sql = $@"
                UPDATE Sessions
                SET {column} = {column} + 1
                WHERE Id = @id
                AND {column} < {maxColumn}
                AND IsActive = 1
                AND IsDeleted = 0";

                // __ Execute the SQL query and check how many rows were affected __ //
                var affected = await _sql.ExecuteAsync(sql, new { id = session.Id });

                // __ If no rows were affected, it means the session was either full, inactive, deleted, or does not exist __ //
                if (affected == 0)
                {
                    await _uow.RollbackTransactionAsync();

                    _logger.LogWarning("Failed to reserve slot for SessionId: {SessionId}, BookingType: {Type}", session.Id, dto.BookingType);

                    return Result.Failure<BookingDto>("Session not available or just booked, try choosing another time");
                }

                // __ Create booking entity __ //
                var booking = new Booking
                {
                    BookingCode = await _codeGen.GenerateAsync("BKN", "BookingCodeSeq"),
                    PatientId = patientId,
                    SessionId = session.Id,
                    ClinicId = followUpClinicId,
                    CaseAssignmentId = lastCase?.Id,
                    BookingType = dto.BookingType,
                    Status = BookingStatus.Confirmed,
                    PreliminaryComplaint = dto.PreliminaryComplaint?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                // __ Add booking to database __ //
                await _uow.Repository<Booking>().AddAsync(booking);

                // __ Single SaveChanges via CommitTransaction __ //
                await _uow.CommitTransactionAsync(patientId.ToString());

                _logger.LogInfo("Booking created {BookingId}", booking.Id);

                // __ Publish domain event for booking creation __ //
                //await _eventPublisher.PublishAsync(new BookingCreatedEvent(booking.Id, booking.SessionId));

                // __ Load full booking details to return in response __ //
                var full = await _uow.Repository<Booking>().GetFirstOrDefaultAsync
                 (
                    b => b.Id == booking.Id,
                    false,
                    b => b.Patient,
                    b => b.Session,
                    b => b.Clinic!
                 );

                // __ Increment patient bookings cache version __ //
                var versionKey = CacheKeys.PatientBookingsVersion(patientId);
                var version = await _cache.GetOrSetAsync(versionKey, _ => Task.FromResult(1));
                await _cache.SetAsync(versionKey, version + 1);

                // __ Invalidate daily metrics so dean dashboard reflects new booking __ //
                await _cache.RemoveAsync(CacheKeys.DailyMetrics);

                return Result.Success(_mapper.Map<BookingDto>(full!));
            }

            // __ Catch any exceptions, log error, rollback transaction, and return failure __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
               
                _logger.LogError("Error creating booking", ex);
                
                return Result.Failure<BookingDto>("An error occurred during booking");
            }
        }
    }
}