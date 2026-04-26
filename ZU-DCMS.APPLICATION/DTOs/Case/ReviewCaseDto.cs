
namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    public class ReviewCaseDto
    {
        public int CaseAssignmentId { get; set; }
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
    }
}