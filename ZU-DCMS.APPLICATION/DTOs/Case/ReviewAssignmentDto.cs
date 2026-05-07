namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    public class ReviewAssignmentDto
    {
        public int CaseAssignmentId { get; set; }
        
        // __ Can be "Approve", "Escalate", or "Transfer" __ //
        public string Action { get; set; } = string.Empty; 
        
        // __ Reason for rejection or transfer __ //
        public string? Notes { get; set; }
    }
}
