using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseReview : BaseEntity
    {
        // _____________ Main Properties _____________ //

        public ReviewStatus Status { get; set; }

        public string? Notes { get; set; }

        public DateTime ReviewedAt { get; set; }

        // _____________ Foriegn Keys _____________ //
        public int CaseAssignmentId { get; set; }
        public int TeachingAssistantId { get; set; } = default!;

        // _____________ Navigation _____________ //
        public TeachingAssistant TeachingAssistant { get; set; } = null!;
        public CaseAssignment CaseAssignment { get; set; } = null!;

    }
}
