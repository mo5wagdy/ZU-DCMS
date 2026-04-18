using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers
{
    public class GetAllUsersHandler
    {
        private readonly IIdentityService _identity;

        public GetAllUsersHandler(IIdentityService identity)
        {
            _identity = identity;
        }

        public async Task<Result<PagedResult<StaffUsersDto>>> Handle(GetAllUsersQuery query)
        {
            var appUsers = _identity.GetAllUsersAsync(query.Request, query.Role);

            if (appUsers is null)
            {
                return Result.Failure<PagedResult<StaffUsersDto>>("خطأ في تحميل المستخدمين");
            }

            return Result.Success<PagedResult<StaffUsersDto>>(appUsers.Result);
        }
    }
}
