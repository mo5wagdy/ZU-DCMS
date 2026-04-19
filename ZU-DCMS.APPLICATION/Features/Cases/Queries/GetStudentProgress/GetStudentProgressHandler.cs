using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentProgress
{
    public class GetStudentProgressHandler : IRequestHandler<GetStudentProgressQuery, Result<StudentProgressDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetStudentProgressHandler> _logger;

        public GetStudentProgressHandler
        (
            IUnitOfWork uow,
            ICacheService cache,
            IMapper mapper,
            IAppLogger<GetStudentProgressHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<StudentProgressDto>> Handle(GetStudentProgressQuery query, CancellationToken cancellationToken)
        {
            var studentId = query.StudentId;
            
            var termId = query.TermId;

            _logger.LogInfo("Calculating progress for student ID: {StudentId} in term ID: {TermId}", studentId, termId);

            // __ Check cache for existing progress data to improve performance and reduce database load __ //
            var cacheKey = CacheKeys.StudentProgress(studentId);

            // __ If cached data exists, return it immediately __ //
            var cached = await _cache.GetAsync<StudentProgressDto>(cacheKey);

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
            if (requirements is null)
            {
                _logger.LogWarning("No requirements found for student ID: {StudentId} in term ID: {TermId}", studentId, termId);
               
                return Result.Failure<StudentProgressDto>("لا يوجد حالات لهذا الطالب");
            }

            // __ Fetch student information to include in the progress DTO __ //
            var student = await _uow.Repository<Student>().GetByIdAsync(studentId);

            // __ If student not found, return failure result __ //
            if (student is null)
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
