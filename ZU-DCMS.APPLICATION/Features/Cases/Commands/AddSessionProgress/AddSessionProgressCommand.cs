using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress
{
    public class AddSessionProgressCommand
    {
        public int StudentId { get; set; }
        public AddCaseSessionDto Dto { get; set; } = null!;
    }
}
