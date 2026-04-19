using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Features.Patients.Commands.UpdateProfile
{
    public record UpdateProfileCommand(int Id, UpdatePatientDto Dto) : IRequest<Result<UpdatePatientDto>>;
}
