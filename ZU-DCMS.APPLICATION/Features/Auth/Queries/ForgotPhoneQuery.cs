
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Queries
{
    public record ForgotPhoneQuery(string nationalId) : IRequest<Result<ForgotPhoneDto>>;
}
