using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllConfigs
{
    public class GetAllConfigsHandler : IRequestHandler<GetAllConfigsQuery, Result<List<SystemConfigDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;

        public GetAllConfigsHandler
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

        public async Task<Result<List<SystemConfigDto>>> Handle(GetAllConfigsQuery query, CancellationToken cancellationToken)
        {
            // __ Fetching From Cache If Available __ //
            var cacheKey = CacheKeys.SystemConfigs;

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => // => If Not Found In Cache Fetch From DB
                {
                    // __ Fetch all configs from SystemConfig table __ //
                    var configs = await _uow.Repository<SystemConfig>().GetListAsync();

                    // __ Map to DTO __ //
                    return Result.Success(_mapper.Map<List<SystemConfigDto>>(configs));
                },
                CacheDuration.Short,
                cancellationToken
            );

            // __ Cache result __ //
            return result!;
        }
    }
}
