using AutoMapper;
using System.Linq.Expressions;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    internal class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRawSqlExecutor _sql;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<StudentService> _logger;

        public StudentService
        (
            IRawSqlExecutor sql,
            IUnitOfWork uow,
            ICacheService cache, 
            IMapper mapper, 
            IAppLogger<StudentService> logger
        )
        {
            _uow = uow;
            _sql = sql;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Result<StudentDto>> GetByIdAsync(int studentId)
        {
            /*
            1. Fetch student by Id
            2. Include:
                - User info
                - Department / Clinic (if exists)
                - Current term data
                - Case assignments summary

            3. If not found → return null
            4. Map to StudentDto
            5. Return result
            */

            _logger.LogInfo("Fetching student with ID {StudentId}.", studentId);

            // __ Fetch student with related data __ //
            var student = await _uow.Repository<Student>().GetFirstOrDefaultAsync
                (
                    st => st.Id == studentId,
                    true,
                    s => s.TermRequirements.Where(tr => tr.Term.IsActive),
                    s => s.CaseAssignments.Where(ca => ca.Clinic.IsActive)
                );

            // __ Handle not found __ //
            if (student is null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found.", studentId);
              
                return Result.Failure<StudentDto>("Student not found.");
            }

            _logger.LogInfo("Student with ID {StudentId} found. Mapping to DTO.", studentId);

            // __ Map to DTO and return __ //
            return Result.Success(_mapper.Map<StudentDto>(student));
        }

        public async Task<Result<StudentDto>> GetByUserIdAsync(string userId)
        {
            /*
            1. Fetch student where Student.UserId == userId
            2. Include same related data as GetByIdAsync
            3. If not found → return null
            4. Map to StudentDto
            5. Return result
            */

            _logger.LogInfo("Fetching student with User ID {UserId}.", userId);

            // __ Fetch student with related data __ //
            var student = await _uow.Repository<Student>().GetFirstOrDefaultAsync
                (
                    st => st.ApplicationUserId == userId,
                    true,
                    s => s.TermRequirements.Where(tr => tr.Term.IsActive),
                    s => s.CaseAssignments.Where(ca => ca.Clinic.IsActive)
                );

            // __ Handle not found __ //
            if (student is null)
            {
                _logger.LogWarning("Student with User ID {UserId} not found.", userId);
               
                return Result.Failure<StudentDto>("Student not found.");
            }

            _logger.LogInfo("Student with User ID {UserId} found. Mapping to DTO.", userId);

            // __ Map to DTO and return __ //
            return Result.Success(_mapper.Map<StudentDto>(student));
        }

        public async Task<Result<PagedResult<StudentDto>>> GetAllAsync(PagedRequest request)
        {
            /*
            1. Build base query from Students table
            2. Apply filters if exists:
                - Search by name / email / code
                - Filter by clinic / department / term (if needed)

            3. Apply pagination:
                - Skip = (Page - 1) * PageSize
                - Take = PageSize

            4. Include:
                - User info
                - Status
                - Progress summary (lightweight only)

            5. Map to StudentDto list

            6. Return PagedResult:
                - Items
                - TotalCount
            */

            _logger.LogInfo("Fetching students with pagination. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            // __ Expression for filtering __ //
            Expression<Func<Student, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();

                filter = s => s.FullName.ToLower().Contains(term) || s.StudentCode.ToLower().Contains(term);
            }

            // __ Func for ordering according to user choice __ // 
            Func<IQueryable<Student>, IOrderedQueryable<Student>> orderBy = q =>
            {
                    var sortBy = request.SortBy?.ToLower();

                    // __ If Descending __ //
                    if (request.SortDescending)
                    {
                        return sortBy switch
                        {
                            "fullname" => q.OrderByDescending(s => s.FullName),
                            "studentcode" => q.OrderByDescending(s => s.StudentCode),
                            "academicyear" => q.OrderByDescending(s => s.AcademicYear),
                            _ => q.OrderByDescending(s => s.Id)
                        };

                    }
                    else // __ If Ascending __ //
                    {
                        return sortBy switch
                        {
                            "fullname" => q.OrderBy(s => s.FullName),
                            "studentcode" => q.OrderBy(s => s.StudentCode),
                            "academicyear" => q.OrderBy(s => s.AcademicYear),
                            _ => q.OrderBy(s => s.Id)
                        };
                    };
             };

            // __ Fetching according to the paginated request using filtering & ordering settings __ //
            var (item, totalCount) = await _uow.Repository<Student>().GetPagedListAsync
                (
                    skip: (request.Page - 1) * request.PageSize,
                    take: (request.PageSize),
                    predicate: filter,
                    orderBy : orderBy,
                    disabledTracking: true
                );

            // __ Mapping to DTO __ //
            var studentDtos = _mapper.Map<List<StudentDto>>(item);

            // __ Creating the paged result __ //
            var pagedResult = PagedResult<StudentDto>.Create(studentDtos, totalCount, request);

            _logger.LogInfo("Fetched {Count} students out of {TotalCount} total students.", studentDtos.Count, totalCount);

            return Result.Success(pagedResult);
        }

        public async Task<Result<List<StudentRequirementDto>>> GetRequirementsAsync(int studentId, int termId)
        {
            /*
            1. Fetch student requirements where:
                - StudentId == studentId
                - TermId == termId

            2. Include:
                - Requirement type
                - Progress status
                - Completion state

            3. Map to StudentRequirementDto
            4. Return list ordered by priority or creation date
            */

            _logger.LogInfo("Fetching Student Requirements");

            // __ Fetching From Cache If Available __ //
            var cacheKey = CacheKeys.StudentRequirements(studentId, termId);

            var cached = await _cache.GetAsync<List<StudentRequirementDto>>(cacheKey);

            if (cached != null)
            {
                _logger.LogInfo("Fetched from cache successfully");

                return Result.Success(cached);
            }

            // __ If Not Found in Cache __ //
            var studentExist = await _uow.Repository<Student>().ExistsAsync(s => s.Id == studentId);

            // __ If Not Found __ //
            if (!studentExist)
            {
                _logger.LogWarning("Error while fetcheing student data");

                return Result.Failure<List<StudentRequirementDto>>("الطالب غير موجود");
            }

            // __ Fetching requirement for the student __ //
            var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                (
                    r => r.StudentId == studentId && r.TermId == termId,
                    true,
                    r => r.Clinic
                );

            if (!requirements.Any())
            {
                _logger.LogWarning("Student Requirements Not Found");

                return Result.Failure<List<StudentRequirementDto>>("لا يوجد متطلبات لهذا الطالب");
            }


            // __ Mapping to DTO __ //
            var dtos = _mapper.Map<List<StudentRequirementDto>>(requirements);

            // __ Refreshing the cache __ //
            await _cache.SetAsync(cacheKey, dtos, CacheDuration.Short);

            _logger.LogInfo("Student Requirement Fetched Successfully");

            return Result.Success(dtos);
        }

        // __ Atomically Increamenting student requirement for the assigned case __ //
        public async Task<Result> IncrementRequirementAsync(int studentId, int clinicId, int termId)
        {
            _logger.LogInfo("Incrementing requirement for student {StudentId}", studentId);

            var sql = @"
            UPDATE TermRequirements
            SET CompletedCount = CompletedCount + 1
            WHERE StudentId = @studentId
            AND ClinicId = @clinicId
            AND TermId = @termId";

            var rows = await _sql.ExecuteAsync(sql, new { studentId, clinicId, termId });

            if(rows == 0)
            {
                _logger.LogWarning("No Rows Affected");

                return Result.Failure("لم تتم زيادة المتطلبات");
            }

            _logger.LogInfo("Incremented requirement for student {StudentId}", studentId);

            return Result.Success();
        }
    }
}