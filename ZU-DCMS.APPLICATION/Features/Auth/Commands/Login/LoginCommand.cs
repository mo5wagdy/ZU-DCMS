using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginPatientDto Dto) : IRequest<Result<AuthDto>>;
}
 