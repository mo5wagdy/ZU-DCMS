using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.SubmitCaseForReview
{
    public record SubmitCaseForReviewCommand(int StudentId, int CaseAssignmentId) : IRequest<Result>;
}
