using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm
{
    public class CreateTermCommand
    {
        public CreateTermDto Dto { get; set; } = null!;
        public string AdminId { get; set; } = null!;
    }
}
