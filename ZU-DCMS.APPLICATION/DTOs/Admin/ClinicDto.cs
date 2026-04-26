namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    public class ClinicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int MinAcademicYear { get; set; }
        public int MaxAcademicYear { get; set; }
        public bool IsActive { get; set; }
    }
}
