
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.ForgotPhone
{
    public record ForgotPhoneCommand(string NationalId) : IRequest<Result<ForgotPhoneResponseDto>>;
}
