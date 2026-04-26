using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetDiagnosisTypes
{
    public record GetDiagnosisTypesQuery(int? ClinicId) : IRequest<Result<List<DiagnosisTypeDto>>>;

    public class DiagnosisTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ClinicId { get; set; }
    }
}
