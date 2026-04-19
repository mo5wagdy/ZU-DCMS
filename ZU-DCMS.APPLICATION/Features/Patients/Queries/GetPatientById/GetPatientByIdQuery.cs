using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientById
{
    public record GetPatientByIdQuery(int Id) : IRequest<Result<PatientDto>>;
}
