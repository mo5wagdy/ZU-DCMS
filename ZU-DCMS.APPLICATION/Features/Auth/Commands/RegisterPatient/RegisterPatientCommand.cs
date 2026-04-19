using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.RegisterPatient
{
    public record RegisterPatientCommand(RegisterPatientDto Dto) : IRequest<Result<AuthDto>>;
}
