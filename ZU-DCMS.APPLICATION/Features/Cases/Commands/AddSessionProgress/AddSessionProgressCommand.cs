using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress
{
    public record AddSessionProgressCommand(int StudentId, int termId, AddCaseSessionDto Dto) : IRequest<Result<CaseSessionDto>>;
}
