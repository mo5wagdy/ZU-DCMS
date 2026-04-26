using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetStudentRequirements
{
    public class GetStudentRequirementsHandler : IRequestHandler<GetStudentRequirementsQuery, Result<List<StudentRequirementDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;

        public GetStudentRequirementsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            IMapper mapper
        )
        {
            _uow = uow;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<Result<List<StudentRequirementDto>>> Handle(GetStudentRequirementsQuery query, CancellationToken cancellationToken)
        {
            // __ Fetching From Cache If Available __ //
            var cacheKey = CacheKeys.StudentRequirements(query.StudentId, query.TermId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => // => If Not Found In Cache Fetch From DB
                {
                    // __ Fetch requirements where studentId + termId __ //
                    var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                        (r =>
                            r.StudentId == query.StudentId &&
                            r.TermId == query.TermId,
                            false,
                            r => r.Clinic
                        );

                    // __ Map to DTO __ //
                    return Result.Success(_mapper.Map<List<StudentRequirementDto>>(requirements));
                },
                CacheDuration.Medium,
                cancellationToken
            );

            // __ Cache result __ //
            return result!;
        }
    }
}