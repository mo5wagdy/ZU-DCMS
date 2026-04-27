using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetProcedures
{
    public record GetProceduresQuery(int? ClinicId) : IRequest<Result<List<ProcedureDto>>>;
}
