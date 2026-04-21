using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewCase
{
    public record ReviewCaseCommand(string TeachingAssistantId, ReviewCaseDto dto) : IRequest<Result>;
}
