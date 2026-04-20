using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers
{
    public record GetAllUsersQuery(PagedRequest Request, string Role) : IRequest<Result<PagedResult<StaffUsersDto>>>;
}
