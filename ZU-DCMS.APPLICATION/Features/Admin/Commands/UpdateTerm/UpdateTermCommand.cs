using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm
{
    public class UpdateTermCommand
    {
        public int TermId { get; set; }
        public UpdateTermDto Dto { get; set; } = null!;
        public string AdminId { get; set; } = null!;
    }
}
