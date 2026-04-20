using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig
{
    public record UpdateConfigCommand(string Key, string Value, string AdminId) : IRequest<Result>;
}
