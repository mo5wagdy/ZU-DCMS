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

            _logger.LogInfo("Fetching cases for student with ID: {StudentId}", studentId);

            // __ Fetch cases with related data __
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

            // __ Handle no cases found __
            if (cases is null)
            {
                _logger.LogWarning("No cases found for student with ID: {StudentId}", studentId);

                return Result.Failure<List<CaseAssignmentDto>>("العيادات غير موجوده");
            }

            _logger.LogInfo("Successfully fetched {Count} cases for student with ID: {StudentId}", cases.Count, studentId);
            
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

            _logger.LogInfo("Fetching case assignment with ID: {CaseAssignmentId}", caseAssignmentId);

            // __ Fetch case assignment with related data __
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

            // __ Handle case assignment not found __
            if (assignment is null)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} not found", caseAssignmentId);

                return Result.Failure<CaseAssignmentDto>("الحالة غير موجودة");
            }

            _logger.LogInfo("Successfully fetched case assignment with ID: {CaseAssignmentId}", caseAssignmentId);

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

            _logger.LogInfo("Adding session progress for student ID: {StudentId} on case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);


            // __ Validates if the case is being marked as completed and has follow-up at the same time, which is not allowed __ //
            if (dto.IsCompleted && dto.HasFollowUp)
            {
                _logger.LogWarning("Cannot mark case assignment ID: {CaseAssignmentId} as completed and has follow-up at the same time", dto.CaseAssignmentId);

                return Result.Failure<CaseSessionDto>("لا يمكن إنهاء الحالة وطلب متابعة في نفس الوقت");
            }

            // __ Validates if at least one procedure is selected for the session, as it's required to track the work done in the session __ //
            if (dto.ProcedureIds is null || dto.ProcedureIds.Count == 0)
            {
                _logger.LogWarning("No procedures selected for case assignment ID: {CaseAssignmentId}", dto.CaseAssignmentId);

                return Result.Failure<CaseSessionDto>("لازم تختار إجراءات");
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

                return Result.Failure<CaseSessionDto>("الحالة غير موجودة");
            }

            // __ Verify that the student has permission to update this case assignment __ //
            if (assignment.StudentId != studentId)
            {
                _logger.LogWarning("Student with ID: {StudentId} does not have permission for case assignment ID: {CaseAssignmentId}", studentId, dto.CaseAssignmentId);

                return Result.Failure<CaseSessionDto>("ليس لديك صلاحية");
            }

            // __ Verify that the case assignment is active and can be updated __ //
            if (assignment.Status != CaseStatus.Active)
            {
                _logger.LogWarning("Case assignment with ID: {CaseAssignmentId} is not active", dto.CaseAssignmentId);

                return Result.Failure<CaseSessionDto>("الحالة غير نشطة");
            }

            // __ Validates that all selected procedures are valid and belong to the clinic associated with the case assignment __ //
            var validCount = await _uow.Repository<Procedure>().CountAsync
                (
                    p => dto.ProcedureIds.Contains(p.Id) &&
                         p.ClinicId == assignment.ClinicId &&
                         p.IsActive
                );

            // __ If the count of valid procedures does not match the count of provided procedure IDs, it means some IDs are invalid __ //
            if (validCount != dto.ProcedureIds.Count) 
            {
                _logger.LogWarning("Invalid procedures selected for case assignment ID: {CaseAssignmentId}", dto.CaseAssignmentId);

                return Result.Failure<CaseSessionDto>("إجراءات غير صحيحة");
            }

            // __ Begin transaction to ensure atomicity of the operation __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Create new case session with provided details __ //
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

                // __ Save the new session to the database __ //
                await _uow.Repository<CaseSession>().AddAsync(session);
                await _uow.SaveChangesAsync();

                // __ Create associations between the session and the selected procedures __ //
                var sessionProcedures = dto.ProcedureIds.Select(id =>
                    new CaseSessionProcedure
                    {
                        CaseSessionId = session.Id,
                        ProcedureId = id,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                // __ Save the session procedures to the database __ //
                await _uow.Repository<CaseSessionProcedure>().AddRangeAsync(sessionProcedures);
                await _uow.SaveChangesAsync();

                // __ If the session is marked as completed, update the case assignment status and increment the student's requirement count for the clinic __ //
                if (dto.IsCompleted)
                {
                    assignment.Status = CaseStatus.Completed;
                   
                    _uow.Repository<CaseAssignment>().Update(assignment);

                    await _studentService.IncrementRequirementAsync(
                        studentId,
                        assignment.ClinicId,
                        assignment.Student.ActiveTermId!.Value);
                }

                // __ Commit The Transaction If Successful __ //
                await _uow.CommitTransactionAsync();

                // __ Handle post-session business rules and notifications __ //
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

            // __ Rollback the transaction in case of any exceptions to maintain data integrity __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
               
                _logger.LogError("Error in AddSessionProgress", ex);

                return Result.Failure<CaseSessionDto>("خطأ أثناء التنفيذ");
            }
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

            _logger.LogInfo("Calculating progress for student ID: {StudentId} in term ID: {TermId}", studentId, termId);

            // __ Check cache for existing progress data to improve performance and reduce database load __ //
            var cacheKey = CacheKeys.StudentProgress(studentId);

            // __ If cached data exists, return it immediately __ //
            var cached = await _cache.GetAsync<StudentProgressDto>(cacheKey);

            // __ If cached data is found, return it to the caller __ //
            if (cached != null)
                return Result.Success(cached);

            // __ If no cached data, proceed to calculate progress from the database __ //
            var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                (
                    r => r.StudentId == studentId && r.TermId == termId,
                    true,
                    r => r.Clinic
                );

            // __ If no requirements are found for the student and term, return a failure result __ //
            if(requirements is null)
            {
                _logger.LogWarning("No requirements found for student ID: {StudentId} in term ID: {TermId}", studentId, termId);
              
                return Result.Failure<StudentProgressDto>("لا يوجد متطلبات لهذا الفصل");
            }

            // __ Fetch student information to include in the progress DTO __ //
            var student = await _uow.Repository<Student>().GetByIdAsync(studentId);

            // __ If student not found, return failure result __ //
            if(student is null)
            {
                _logger.LogWarning("Student not found for ID: {StudentId}", studentId);
             
                return Result.Failure<StudentProgressDto>("الطالب غير موجود");
            }

            // __ Calculate overall progress metrics based on the requirements data __ //
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

            // __ Cache the calculated progress result for future requests to improve performance __ //
            await _cache.SetAsync(cacheKey, result, CacheDuration.Short);

            _logger.LogInfo("Successfully calculated progress for student ID: {StudentId} in term ID: {TermId}", studentId, termId);

            return Result.Success(result);
        }
    }
}