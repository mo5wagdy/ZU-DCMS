using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetSessionPatients
{
    public record GetSessionPatientsQuery(int SessionId, string InternDoctorId) : IRequest<Result<PagedResult<BookingForDiagnosisDto>>>;
}
