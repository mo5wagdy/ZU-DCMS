namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetActiveTerm
{
    public class SetActiveTermCommand
    {
        public int TermId { get; set; }
        public string AdminId { get; set; } = null!;
    }
}
