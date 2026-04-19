using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientByUserId
{
    public record GetPatientByUserIdQuery(string UserId) : IRequest<Result<PatientDto>>;
}
