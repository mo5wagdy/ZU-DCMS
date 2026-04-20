using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetActiveTerm
{
    public record SetActiveTermCommand(int TermId, string AdminId) : IRequest<Result>;
}
