
namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // __ DTO for adding a new Case Session __ //
    public class AddCaseSessionDto
    {
        public int CaseAssignmentId { get; set; }
        public List<int> ProcedureIds { get; set; } = new();
        public bool IsCompleted { get; set; }
        public bool HasFollowUp { get; set; }
        public string? Notes { get; set; }
    }
}
