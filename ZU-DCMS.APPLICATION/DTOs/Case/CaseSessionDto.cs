
namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // __ DTO for Case Session information __ //
    public class CaseSessionDto
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public List<string> ProceduresNames { get; set; } = new();
        public bool IsCompleted { get; set; }
        public bool HasFollowUp { get; set; }
        public string? Notes { get; set; }
        public DateTime SessionDate { get; set; }
    }
}
