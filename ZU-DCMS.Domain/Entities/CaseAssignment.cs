using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseAssignment : BaseEntity
    {
        // Properties
        public int DiagnosisRecordId { get; set; }
        public int StudentId { get; set; }
        public int ClinicId { get; set; }
        public int AssignedByInternId { get; set; }
        public CaseStatus Status { get; set; } = CaseStatus.Active;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // Navigation
        public InternDoctor AssignedByIntern { get; set; } = null!;
        public DiagnosisRecord DiagnosisRecord { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
        public ICollection<CaseSession> Sessions { get; set; } = new List<CaseSession>();
    }
}
