using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetReviewedAssignments
{
    public record GetReviewedAssignmentsQuery(string TeachingAssistantUserId) : IRequest<Result<List<CaseAssignmentDto>>>;
}
