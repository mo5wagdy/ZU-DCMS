using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseSession : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public DateTime SessionDate { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;
        public bool HasFollowUp { get; set; }
        public string? Notes { get; set; }
        public string? InternApprovalId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // _____________ Foreign Keys _____________ //
        public int CaseAssignmentId { get; set; }
        public int StudentId { get; set; }
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public CaseAssignment CaseAssignment { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
        public ICollection<CaseSessionProcedure> Procedures { get; set; } = new List<CaseSessionProcedure>();
    }
}
