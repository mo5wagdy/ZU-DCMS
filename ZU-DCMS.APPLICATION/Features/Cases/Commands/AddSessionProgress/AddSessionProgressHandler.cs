using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress
{
    public class AddSessionProgressHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IStudentService _studentService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IAppLogger<AddSessionProgressHandler> _logger;

        public AddSessionProgressHandler(
            IUnitOfWork uow,
            IStudentService studentService,
            IEventPublisher eventPublisher,
            IMapper mapper,
            IAppLogger<AddSessionProgressHandler> logger)
        {
            _uow = uow;
            _studentService = studentService;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CaseSessionDto>> Handle(AddSessionProgressCommand command)
        {
            var studentId = command.StudentId;
            var dto = command.Dto;

            _logger.LogInfo("Adding session progress for student ID: {StudentId} on case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);

            if (dto.IsCompleted && dto.HasFollowUp)
            {
                _logger.LogWarning("Cannot mark case assignment ID: {CaseAssignmentId} as completed and has follow-up at the same time", dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("لا يمكن إنهاء الحالة وطلب متابعة في نفس الوقت");
            }

            if (dto.ProcedureIds is null || dto.ProcedureIds.Count == 0)
            {
                _logger.LogWarning("No procedures selected for case assignment ID: {CaseAssignmentId}", dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("لازم تختار إجراءات");
            }

            var assignment = await _uow.Repository<CaseAssignment>().GetFirstOrDefaultAsync
                (
                    c => c.Id == dto.CaseAssignmentId,
                    false,
                    c => c.Student,
                    c => c.Clinic,
                    c => c.DiagnosisRecord.Booking.Patient
                );

            if (assignment is null)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} not found", dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("الحالة غير موجودة");
            }

            if (assignment.StudentId != studentId)
            {
                _logger.LogWarning("Student with ID: {StudentId} does not have permission for case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("ليس لديك صلاحية");
            }

            if (assignment.Status != CaseStatus.Active)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} is not active", dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("الحالة غير نشطة");
            }

            var validCount = await _uow.Repository<Procedure>().CountAsync
                (
                    p => dto.ProcedureIds.Contains(p.Id) &&
                         p.ClinicId == assignment.ClinicId &&
                         p.IsActive
                );

            if (validCount != dto.ProcedureIds.Count) 
            {
                _logger.LogWarning("Invalid procedures selected for case assignment ID: {CaseAssignmentId}", dto.CaseAssignmentId);
                return Result.Failure<CaseSessionDto>("إجراءات غير صحيحة");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                var session = new CaseSession
                {
                    CaseAssignmentId = dto.CaseAssignmentId,
                    StudentId = studentId,
                    IsCompleted = dto.IsCompleted,
                    HasFollowUp = dto.HasFollowUp,
                    Notes = dto.Notes?.Trim(),
                    SessionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Repository<CaseSession>().AddAsync(session);
                await _uow.SaveChangesAsync();

                var sessionProcedures = dto.ProcedureIds.Select(id =>
                    new CaseSessionProcedure
                    {
                        CaseSessionId = session.Id,
                        ProcedureId = id,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                await _uow.Repository<CaseSessionProcedure>().AddRangeAsync(sessionProcedures);
                await _uow.SaveChangesAsync();

                if (dto.IsCompleted)
                {
                    assignment.Status = CaseStatus.Completed;
                    _uow.Repository<CaseAssignment>().Update(assignment);

                    await _studentService.IncrementRequirementAsync(
                        studentId,
                        assignment.ClinicId,
                        assignment.Student.ActiveTermId!.Value);
                }

                await _uow.CommitTransactionAsync();

                if (dto.IsCompleted)
                {
                    await _eventPublisher.PublishAsync(new CaseCompletedEvent(studentId, assignment.Id, assignment.ClinicId));
                }

                if (dto.HasFollowUp)
                {
                    await _eventPublisher.PublishAsync(new CasePartiallyCompletedEvent(studentId, assignment.Id, assignment.ClinicId));
                }

                _logger.LogInfo("Successfully added session progress for student ID: {StudentId} on case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);

                return Result.Success(_mapper.Map<CaseSessionDto>(session));
            }
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("Error in AddSessionProgress", ex);
                return Result.Failure<CaseSessionDto>("خطأ أثناء التنفيذ");
            }
        }
    }
}
