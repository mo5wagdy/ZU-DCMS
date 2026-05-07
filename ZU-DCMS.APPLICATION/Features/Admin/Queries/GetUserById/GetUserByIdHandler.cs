using AutoMapper;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<StaffUsersDto>>
    {
        private readonly IIdentityService _identity;
        private readonly IFusionCache _cache;
        private readonly IMapper _mapper;

        public GetUserByIdHandler
        (
            IIdentityService identity,
            IFusionCache cache,
            IMapper mapper
        )
        {
            _identity = identity;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<Result<StaffUsersDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            // __ Fetching From Cache If Available __ //
            var cacheKey = CacheKeys.StaffUserByUserId(query.UserId);

            var result = await _cache.GetOrSetAsync
            (
                cacheKey,
                async _ => // => If Not Found In Cache Fetch From DB
                {
                    // __ Fetch user by Id __ //
                    var user = await _identity.FindByIdAsync(query.UserId);

                    // __ If null → return null __ //
                    if (user is null)
                        return Result.Failure<StaffUsersDto>("User not found");

                    var roles = await _identity.GetRolesAsync(query.UserId);

                    var userRoles = roles.FirstOrDefault();

                    var result = new StaffUsersDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Username = user.Username,
                        Email = user.Email,
                        Role = userRoles ?? string.Empty,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    // __ Return DTO __ //
                    return Result.Success(result);
                },
                CacheDuration.Short,
                cancellationToken
            );

            // __ Cache result __ //
            return result!;
        }
    }
}
