using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Student.Queries.GetRequirements
{
    public class GetRequirementsHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetRequirementsHandler> _logger;

        public GetRequirementsHandler(IUnitOfWork uow, ICacheService cache, IMapper mapper, IAppLogger<GetRequirementsHandler> logger)
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<StudentRequirementDto>>> Handle(GetRequirementsQuery query)
        {
            var studentId = query.StudentId;
            var termId = query.TermId;

            _logger.LogInfo("Fetching Student Requirements");

            var cacheKey = CacheKeys.StudentRequirements(studentId, termId);
            var cached = await _cache.GetAsync<List<StudentRequirementDto>>(cacheKey);

            if (cached != null)
            {
                _logger.LogInfo("Fetched from cache successfully");
                return Result.Success(cached);
            }

            var studentExist = await _uow.Repository<Domain.Entities.Student>().ExistsAsync(s => s.Id == studentId);

            if (!studentExist)
            {
                _logger.LogWarning("Error while fetcheing student data");
                return Result.Failure<List<StudentRequirementDto>>("الطالب غير موجود");
            }

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

            var dtos = _mapper.Map<List<StudentRequirementDto>>(requirements);

            await _cache.SetAsync(cacheKey, dtos, CacheDuration.Short);

            _logger.LogInfo("Student Requirement Fetched Successfully");

            return Result.Success(dtos);
        }
    }
}
