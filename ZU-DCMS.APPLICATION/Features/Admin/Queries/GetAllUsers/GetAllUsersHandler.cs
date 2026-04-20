using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<StaffUsersDto>>>
    {
        private readonly IIdentityService _identity;

        public GetAllUsersHandler(IIdentityService identity)
        {
            _identity = identity;
        }

        public async Task<Result<PagedResult<StaffUsersDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
        {
            // __ Build base query from Users table __ //
            var appUsers = _identity.GetAllUsersAsync(query.Request, query.Role);

            if (appUsers is null)
            {
                return Result.Failure<PagedResult<StaffUsersDto>>("خطأ في تحميل المستخدمين");
            }

            // __ Map to StaffUsersDto __ //
            return Result.Success<PagedResult<StaffUsersDto>>(appUsers.Result);
        }
    }
}
