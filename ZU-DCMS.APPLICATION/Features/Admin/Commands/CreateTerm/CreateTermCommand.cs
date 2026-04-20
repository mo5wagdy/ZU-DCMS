using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm
{
    public record CreateTermCommand(CreateTermDto Dto, string AdminId) : IRequest<Result<TermDto>>;
}
