using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetActiveTerm
{
    public record GetActiveTermQuery() : IRequest<Result<TermDto>>;
}
