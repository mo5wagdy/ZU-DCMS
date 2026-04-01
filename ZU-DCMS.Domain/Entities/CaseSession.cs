using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseSession : BaseEntity
    {
        public int CaseAssignmentId { get; set; }
        public int StudentId { get; set; }
        public DateTime SessionDate { get; set; } = DateTime.UtcNow;
        public string ProceduresDone { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public string? Notes { get; set; }
        public string? InternApprovalId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Navigation
        public CaseAssignment CaseAssignment { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
