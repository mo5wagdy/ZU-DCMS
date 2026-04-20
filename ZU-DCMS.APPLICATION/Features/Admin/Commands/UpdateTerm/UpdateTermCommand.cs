using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm
{
    public record UpdateTermCommand(int TermId, UpdateTermDto Dto, string AdminId) : IRequest<Result<TermDto>>; 
}
