using AutoMapper;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    internal class CaseService : ICaseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IStudentService _studentService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<CaseService> _logger;

        public CaseService(
            IUnitOfWork uow,
            IStudentService studentService,
            IEventPublisher eventPublisher,
            ICacheService cache,
            IMapper mapper,
            IAppLogger<CaseService> logger)
        {
            _uow = uow;
            _studentService = studentService;
            _eventPublisher = eventPublisher;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Result<List<CaseAssignmentDto>>> GetStudentCasesAsync(int studentId)
        {
            /*
            1. Fetch CaseAssignments where StudentId == studentId
            2. Include related Case + Status + Progress
            3. Map to CaseAssignmentDto
            4. Return list
            */
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync
            (
                c => c.StudentId == studentId,
                true,
                c => c.DiagnosisRecord,
                c => c.DiagnosisRecord.Booking.Patient,
                c => c.DiagnosisRecord.DiagnosisType,
                c => c.Clinic,
                c => c.AssignedByIntern,
                c => c.Sessions
            );

            if(cases is null)
            {
                _logger.LogWarning("");

                return Result.Failure<List<CaseAssignmentDto>>("العيادات غير موجوده");
            }

            return Result.Success(_mapper.Map<List<CaseAssignmentDto>>(cases));
        }

        public async Task<Result<CaseAssignmentDto>> GetByIdAsync(int caseAssignmentId)
        {
            /*
            1. Fetch CaseAssignment by id
            2. If null → return null
            3. Include related data (Case, Sessions, Procedures)
            4. Map to CaseAssignmentDto
            5. Return DTO
            */

            _logger.LogInfo("");

            var assignment = await _uow.Repository<CaseAssignment>().GetFirstOrDefaultAsync
                (
                    c => c.Id == caseAssignmentId,
                    false,
                    c => c.DiagnosisRecord,
                    c => c.DiagnosisRecord.Booking.Patient,
                    c => c.DiagnosisRecord.DiagnosisType,
                    c => c.Clinic,
                    c => c.AssignedByIntern,
                    c => c.Sessions
                );

            if (assignment is null)
            {
                _logger.LogWarning("");

                return Result.Failure<CaseAssignmentDto>("الحالة غير موجودة");
            }

            return Result.Success(_mapper.Map<CaseAssignmentDto>(assignment));
        }

        public async Task<Result<CaseSessionDto>> AddSessionProgressAsync(int studentId, AddCaseSessionDto dto)
        {
            /*
            1. Validate DTO (fluent validation or manual)
            2. Fetch CaseAssignment by dto.CaseAssignmentId
            3. If null → throw NotFound
            4. Verify CaseAssignment.StudentId == studentId → else Forbidden
            5. Verify CaseAssignment.Status == Active → else BadRequest
            6. Fetch Procedures by dto.ProcedureIds
            7. If missing procedures → throw NotFound

            8. Begin Transaction

            9. Create CaseSession entity
            10. Create CaseSessionProcedures
            11. Save session + procedures

            12. Check business rules:
                ├── If IsCompleted:
                │     a. CaseAssignment.Status = Completed
                │     b. Increment TermRequirement.CompletedCount
                │     c. Send CaseCompleted notification
                │     d. Emit SignalR StatsUpdated event
                │
                ├── Else if HasFollowUp:
                │     a. Send CasePartiallyCompleted notification
                │     b. Emit SignalR StatsUpdated event

            13. SaveChanges

            14. Commit transaction

            15. Invalidate cache:
                - StudentProgress(studentId)
                - AvailableStudents(clinicId)

            16. Return CaseSessionDto
            */
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
                _logger.LogWarning("");

                return Result.Failure<CaseSessionDto>("الحالة غير موجودة");
            }

            if (assignment.StudentId != studentId)
            {
                _logger.LogWarning("");

                return Result.Failure<CaseSessionDto>("ليس لديك صلاحية");
            }

            if (assignment.Status != CaseStatus.Active)
            {
                _logger.LogWarning("");

                return Result.Failure<CaseSessionDto>("الحالة غير نشطة");
            }

            // تحقق من صحة الإجراءات
            var validCount = await _uow.Repository<Procedure>().CountAsync
                (
                    p => dto.ProcedureIds.Contains(p.Id) &&
                         p.ClinicId == assignment.ClinicId &&
                         p.IsActive
                );

            if (validCount != dto.ProcedureIds.Count) 
            {
                _logger.LogWarning("");

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
                await _uow.SaveChangesAsync(); // عشان نجيب ID

                var sessionProcedures = dto.ProcedureIds.Select(id =>
                    new CaseSessionProcedure
                    {
                        CaseSessionId = session.Id,
                        ProcedureId = id,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                await _uow.Repository<CaseSessionProcedure>()
                    .AddRangeAsync(sessionProcedures);

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

                return Result.Success(_mapper.Map<CaseSessionDto>(session));
            }

            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
               
                _logger.LogError("Error in AddSessionProgress", ex);

                return Result.Failure<CaseSessionDto>("خطأ أثناء التنفيذ");
            }
        }
        

        public async Task<Result> CompleteCaseAsync(int caseAssignmentId, int studentId)
        {
            /*
            1. Fetch CaseAssignment by id
            2. If null → NotFound
            3. Verify ownership → studentId match
            4. If already completed → return (idempotent)

            5. Set Status = Completed
            6. Update completion timestamps

            7. Update TermRequirement.CompletedCount

            8. SaveChanges

            9. Send CaseCompleted notification
            10. Emit SignalR StatsUpdated event

            11. Invalidate related cache:
                - StudentProgress
                - Case details cache
            */

            throw new NotImplementedException();
        }

        public async Task<Result<StudentProgressDto>> GetStudentProgressAsync(int studentId, int termId)
        {
            /*
            1. Fetch all CaseAssignments for student in termId
            2. Include status + sessions
            3. Calculate:
                - Total cases
                - Completed cases
                - In progress cases
                - Completion percentage

            4. Map to StudentProgressDto
            5. Return DTO
            */

            var cacheKey = CacheKeys.StudentProgress(studentId);

            var cached = await _cache.GetAsync<StudentProgressDto>(cacheKey);

            if (cached != null)
                return Result.Success(cached);

            var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                (
                    r => r.StudentId == studentId && r.TermId == termId,
                    true,
                    r => r.Clinic
                );

            var student = await _uow.Repository<Student>().GetByIdAsync(studentId);

            var result = new StudentProgressDto
            {
                StudentId = studentId,
                FullName = student!.FullName,
                StudentCode = student.StudentCode,
                TotalRequired = requirements.Sum(x => x.RequiredCount),
                TotalCompleted = requirements.Sum(x => x.CompletedCount),
                TotalTransferred = requirements.Sum(x => x.TransferredCount),
                IsTermComplete = requirements.All(x => x.IsSatisfied),
                Requirements = _mapper.Map<List<StudentRequirementDto>>(requirements)
            };

            await _cache.SetAsync(cacheKey, result, CacheDuration.Short);

            return Result.Success(result);
        }
    }
}