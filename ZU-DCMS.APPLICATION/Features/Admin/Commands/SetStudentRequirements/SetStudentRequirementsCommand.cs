using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements
{
    public class SetStudentRequirementsCommand
    {
        public int StudentId { get; set; }
        public int TermId { get; set; }
        public List<SetRequirementDto> Requirements { get; set; } = new();
        public string AdminId { get; set; } = null!;
    }
}
