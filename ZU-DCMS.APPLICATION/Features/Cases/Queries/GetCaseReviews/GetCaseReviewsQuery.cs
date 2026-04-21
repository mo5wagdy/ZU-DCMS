using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCaseReviews
{
    public record GetCaseReviewsQuery(int CaseAssignmentId) : IRequest<Result<List<ReviewCaseDto>>>;
}
