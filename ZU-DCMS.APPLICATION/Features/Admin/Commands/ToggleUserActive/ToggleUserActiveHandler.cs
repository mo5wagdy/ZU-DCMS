using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.ToggleUserActive
{
    public class ToggleUserActiveHandler
    {
        public async Task<Result> Handle(ToggleUserActiveCommand command)
        {
            return Result.Success();
        }
    }
}
