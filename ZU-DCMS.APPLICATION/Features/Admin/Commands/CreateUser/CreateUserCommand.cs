using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateUser
{
    public record CreateUserCommand(CreateUserDto Dto) : IRequest<Result<StaffUsersDto>>; 
}
