namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    public class ClinicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;    // Legacy Arabic name
        public string NameAr { get; set; } = string.Empty;  // Arabic
        public string NameEn { get; set; } = string.Empty;  // English
        public string Code { get; set; } = string.Empty;
        public int MinAcademicYear { get; set; }
        public int MaxAcademicYear { get; set; }
        public bool IsActive { get; set; }
    }
}
