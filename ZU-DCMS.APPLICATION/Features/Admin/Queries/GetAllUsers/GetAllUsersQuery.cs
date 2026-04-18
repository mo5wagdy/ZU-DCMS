using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers
{
    public class GetAllUsersQuery
    {
        public PagedRequest Request { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
