namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig
{
    public class UpdateConfigCommand
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string AdminId { get; set; } = null!;
    }
}
