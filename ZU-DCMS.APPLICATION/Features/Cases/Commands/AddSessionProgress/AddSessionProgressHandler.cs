using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.SubmitCaseForReview;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress
{
    public class AddSessionProgressHandler : IRequestHandler<AddSessionProgressCommand, Result<CaseSessionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMediator _mediator;
        private readonly IFusionCache _cache;
        private readonly IAppLogger<AddSessionProgressHandler> _logger;

        public AddSessionProgressHandler
        (
            IUnitOfWork uow,
            IMediator mediator,
            IFusionCache cache,
            IAppLogger<AddSessionProgressHandler> logger
        )
        {
            _uow = uow;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<CaseSessionDto>> Handle(AddSessionProgressCommand command, CancellationToken cancellationToken)
        {
            var studentId = command.StudentId;
            var termId = command.TermId;
            var dto = command.Dto;

            _logger.LogInfo("Adding session progress for student ID: {StudentId} on case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);

            // __ Validates if the case is being marked as completed and has follow-up at the same time, which is not allowed __ //
            if (dto.IsCompleted && dto.HasFollowUp)
            {
                _logger.LogWarning("Cannot mark case assignment ID: {CaseAssignmentId} as completed and has follow-up at the same time", dto.CaseAssignmentId);
                
                return Result.Failure<CaseSessionDto>("Cannot complete the case and request follow-up at the same time");
            }

            // __ Validates if at least one procedure is selected for the session, as it's required to track the work done in the session __ //
            if (dto.ProcedureIds is null || dto.ProcedureIds.Count == 0)
            {
                _logger.LogWarning("No procedures selected for case assignment ID: {CaseAssignmentId}", dto.CaseAssignmentId);
                
                return Result.Failure<CaseSessionDto>("Must select procedures");
            }

            // __ Fetch case assignment with related data __ //
            var assignment = await _uow.Repository<CaseAssignment>().GetFirstOrDefaultAsync
                (
                    c => c.Id == dto.CaseAssignmentId,
                    false,
                    c => c.Student,
                    c => c.Clinic,
                    c => c.DiagnosisRecord.Booking.Patient
                );

            // __ Handle case assignment not found __ // 
            if (assignment is null)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} not found", dto.CaseAssignmentId);
                
                return Result.Failure<CaseSessionDto>("Case not found");
            }

            // __ Verify that the student has permission to update this case assignment __ //
            if (assignment.StudentId != studentId)
            {
                _logger.LogWarning("Student with ID: {StudentId} does not have permission for case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);
                
                return Result.Failure<CaseSessionDto>("You do not have permission");
            }

            // __ Only allow progress while case is being worked on __ //
            if (assignment.Status != CaseStatus.InProgress)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} is not active", dto.CaseAssignmentId);
             
                return Result.Failure<CaseSessionDto>("Case is not in progress");
            }

            // __ Validate that the patient has a booking for today (New or Follow-up) __ //
            var egyptTime = DateTime.UtcNow.AddHours(3);
            var today = egyptTime.Date;
            var tomorrow = today.AddDays(1);
            var patientId = assignment.DiagnosisRecord.Booking.PatientId;
            
            var bookingsToday = await _uow.Repository<Booking>().GetListAsync(
                b => b.PatientId == patientId && 
                     b.Session.Date >= today && b.Session.Date < tomorrow &&
                     (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Delayed) &&
                     (b.CaseAssignmentId == assignment.Id || b.Id == assignment.DiagnosisRecord.BookingId),
                false
            );

            if (!bookingsToday.Any())
            {
                _logger.LogWarning("Patient {PatientId} does not have a valid booking for today for case {CaseId}", patientId, dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("المريض ليس لديه حجز (متابعة أو جديد) مسجل لهذا اليوم. يجب على المريض حجز موعد أولاً.");
            }

            // __ Validates that all selected procedures are valid and belong to the clinic associated with the case assignment __ //
            var validCount = await _uow.Repository<ClinicProcedure>().CountAsync
                (
                    p => dto.ProcedureIds.Contains(p.ProcedureId) &&
                    p.ClinicId == assignment.ClinicId &&
                    p.IsActive
                );

            // __ If the count of valid procedures does not match the count of provided procedure IDs, it means some IDs are invalid __ //
            if (validCount != dto.ProcedureIds.Count) 
            {
                _logger.LogWarning("Invalid procedures selected for case assignment ID: {CaseAssignmentId}", dto.CaseAssignmentId);
                
                return Result.Failure<CaseSessionDto>("Invalid procedures");
            }

            // __ Begin transaction to ensure atomicity of the operation __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Mark today's bookings as Completed since the patient attended the session __ //
                foreach (var booking in bookingsToday)
                {
                    booking.Status = BookingStatus.Completed;
                    _uow.Repository<Booking>().Update(booking);
                }

                // __ Create new case session with provided details __ //
                var session = new CaseSession
                {
                    CaseAssignmentId = dto.CaseAssignmentId,
                    StudentId = studentId,
                    ClinicId = assignment.ClinicId,
                    IsCompleted = dto.IsCompleted,
                    HasFollowUp = dto.HasFollowUp,
                    Notes = dto.Notes?.Trim(),
                    SessionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // __ Save the new session to the database __ //
                await _uow.Repository<CaseSession>().AddAsync(session);
                
                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

                // __ Create associations between the session and the selected procedures __ //
                var sessionProcedures = dto.ProcedureIds.Select
                (id =>
                    new CaseSessionProcedure
                    {
                        CaseSessionId = session.Id,
                        ProcedureId = id,
                        CreatedAt = DateTime.UtcNow
                    }
                );

                // __ Save the session procedures to the database __ //
                await _uow.Repository<CaseSessionProcedure>().AddRangeAsync(sessionProcedures);
                
                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

                // __ Commit The Transaction If Successful __ //
                await _uow.CommitTransactionAsync();

                // __ If the session is marked as completed, update the case assignment status and increment the student's requirement count for the clinic __ //
                if (dto.IsCompleted)
                {
                    await _mediator.Send(new SubmitCaseForReviewCommand(studentId, dto.CaseAssignmentId), cancellationToken: cancellationToken);

                    _logger.LogInfo("Case moved to PendingReview - waiting TA approval for student {StudentId}", studentId);
                }

                // __ Handle post-session business rules and notifications __ //
                //if (dto.IsCompleted)
                //{
                //    //await _eventPublisher.PublishAsync(new CaseCompletedEvent(studentId, assignment.Id, assignment.ClinicId));
                //}

                //if (dto.HasFollowUp)
                //{
                //   // await _eventPublisher.PublishAsync(new CasePartiallyCompletedEvent(studentId, assignment.Id, assignment.ClinicId));
                //}

                // __ Cache Invalidation For Data Consistency __ //
                await _cache.RemoveAsync(CacheKeys.StudentCases(studentId));
                await _cache.RemoveAsync(CacheKeys.CaseById(assignment.Id));
                await _cache.RemoveAsync(CacheKeys.AvailableStudents(assignment.ClinicId));

                // __ Fetch procedure names safely from the database instead of relying on unloaded EF navigations __ //
                var procedures = await _uow.Repository<Procedure>().GetListAsync(p => dto.ProcedureIds.Contains(p.Id));

                var resultDto = new CaseSessionDto
                {
                    Id = session.Id,
                    ClinicId = session.ClinicId,
                    IsCompleted = session.IsCompleted,
                    HasFollowUp = session.HasFollowUp,
                    Notes = session.Notes,
                    SessionDate = session.SessionDate,
                    ProceduresNames   = procedures.Select(p => p.NameAr).ToList(),
                    ProceduresNamesEn = procedures.Select(p => p.NameEn).ToList()
                };

                _logger.LogInfo("Successfully added session progress for student ID: {StudentId} on case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);

                return Result.Success(resultDto);
            }

            // __ Rollback the transaction in case of any exceptions to maintain data integrity __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("Error in AddSessionProgress", ex);
                
                return Result.Failure<CaseSessionDto>("Error during execution");
            }
        }
    }
}