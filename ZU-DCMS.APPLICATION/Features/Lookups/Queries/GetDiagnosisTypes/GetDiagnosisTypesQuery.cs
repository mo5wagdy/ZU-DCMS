using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetDiagnosisTypes
{
    public record GetDiagnosisTypesQuery(int? ClinicId) : IRequest<Result<List<DiagnosisTypeDto>>>;
}
