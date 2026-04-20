using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetUserById
{
    public record GetUserByIdQuery(string UserId) : IRequest<Result<StaffUsersDto>>;
}
