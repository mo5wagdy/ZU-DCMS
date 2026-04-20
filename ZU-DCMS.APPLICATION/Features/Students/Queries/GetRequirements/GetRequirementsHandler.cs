using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Students.Queries.GetRequirements
{
    public class GetRequirementsHandler : IRequestHandler<GetRequirementsQuery, Result<List<StudentRequirementDto>>>
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


        public async Task<Result<List<StudentRequirementDto>>> Handle(GetRequirementsQuery query, CancellationToken cancellationToken)
        {
            var studentId = query.StudentId;
            var termId = query.TermId;

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
            var studentExist = await _uow.Repository<Domain.Entities.Student>().ExistsAsync(s => s.Id == studentId);

            // __ If Student Not Found __ //
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

            // __ If no requirement found __ //
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
    }
}
