using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.StaffLogin
{
    public record StaffLoginCommand(StaffLoginDto Dto) : IRequest<Result<AuthDto>>;
}
