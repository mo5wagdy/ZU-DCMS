using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewAssignment
{
    public class ReviewAssignmentCommand : IRequest<Result<string>>
    {
        public string TaUserId { get; set; } = string.Empty;
        public ReviewAssignmentDto Dto { get; set; } = null!;
    }
}
