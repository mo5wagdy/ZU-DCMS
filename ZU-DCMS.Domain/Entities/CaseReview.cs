using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseReview : BaseEntity
    {
        public int CaseAssignmentId { get; set; }

        public string TeachingAssistantId { get; set; } = default!;

        public ReviewStatus Status { get; set; }

        public string? Notes { get; set; }

        public DateTime ReviewedAt { get; set; }
    }
}
