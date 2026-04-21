
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseAssignment : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public CaseStatus Status { get; set; } = CaseStatus.Active;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // _____________ Foreign Keys _____________ //
        public int AssignedByInternId { get; set; }
        public int DiagnosisRecordId { get; set; }
        public int StudentId { get; set; }
        public int TermId { get; set; }
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public InternDoctor AssignedByIntern { get; set; } = null!;
        public DiagnosisRecord DiagnosisRecord { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
        public Term Term { get; set; } = null!
        public ICollection<CaseSession> Sessions { get; set; } = new List<CaseSession>();
    }
}
