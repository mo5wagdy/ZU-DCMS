using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCasesForReview
{
    public record GetCasesForReviewQuery() : IRequest<Result<List<CaseAssignmentDto>>>;
}
