using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<StaffUsersDto>>>
    {
        private readonly IIdentityService _identity;
        private readonly IFusionCache _cache;

        public GetAllUsersHandler
        (
            IIdentityService identity,
            IFusionCache cache
        )
        {
            _identity = identity;
            _cache = cache;
        }

        public async Task<Result<PagedResult<StaffUsersDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
        {
            // __ Fetching From Cache If Available __ //
            var cacheKey = CacheKeys.StaffUsers;

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => // => If Not Found In Cache Fetch From DB
                {
                    // __ Build base query from Users table __ //
                    var appUsers = await _identity.GetAllUsersAsync(query.Request, query.Role);

                    if (appUsers is null)
                    {
                        return Result.Failure<PagedResult<StaffUsersDto>>("خطأ في تحميل المستخدمين");
                    }

                    // __ Map to StaffUsersDto __ //
                    return Result.Success(appUsers);
                },
                CacheDuration.Medium,
                cancellationToken
            );

            // __ Cache result __ //
            return result!;
        }
    }
}