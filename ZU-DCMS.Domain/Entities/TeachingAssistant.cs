using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class TeachingAssistant : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string ApplicationUserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string TACode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // _____________ Navigation _____________ //
        public ICollection<CaseReview> CaseReviews { get; set; } = new List<CaseReview>();
        
        // __ Assignments reviewed by this TA during the pre-treatment phase __ //
        public ICollection<CaseAssignment> ReviewedAssignments { get; set; } = new List<CaseAssignment>();
    }
}
