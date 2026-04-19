using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetAllPatients
{
    public record GetAllPatientsQuery(PagedRequest Request) : IRequest<Result<PagedResult<PatientDto>>>;
}
