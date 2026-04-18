namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.TransferRequirements
{
    public class TransferRequirementsCommand
    {
        public int StudentId { get; set; }
        public int FromTermId { get; set; }
        public int ToTermId { get; set; }
        public string AdminId { get; set; } = null!;
    }
}
