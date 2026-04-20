using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetTermById
{
    public record GetTermByIdQuery(int TermId) : IRequest<Result<TermDto>>;
}
