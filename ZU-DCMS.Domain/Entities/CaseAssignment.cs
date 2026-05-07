
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
        // __ CaseAssignment represents the assignment of a diagnosed patient case to a student. __ //
    public class CaseAssignment : BaseEntity
    {
        // _____________ Main Properties _____________ //
        // __ Current status of the case assignment. __ //
        public CaseStatus Status { get; set; } = CaseStatus.InProgress;

        // __ Timestamp when this case was assigned to the student. __ //
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // __ Timestamp when this case was successfully completed and approved by the TA. __ //
        public DateTime? CompletedAt { get; set; }

        // __ Optional notes about the assignment (e.g., special instructions, constraints). __ //
        public string? Notes { get; set; }

        // _____________ Pre-Treatment Assignment Review Tracking _____________ //
        
        // __ لمعرفة هل النظام هو من اختار الطالب أم لا (تلقائي) __ //
        public bool IsAutoAssigned { get; set; } = false;

        // __ وقت مراجعة المعيد للتعيين __ //
        public DateTime? AssignmentReviewedAt { get; set; }

        // __ ملاحظات المعيد عند القبول أو الرفض أو التحويل __ //
        public string? ReviewNotes { get; set; }

        // _____________ Foreign Keys _____________ //
        // __ The InternDoctor who assigned this case. __ //
        public int AssignedByInternId { get; set; }

        // __ المعيد الذي قام بمراجعة التعيين (إذا تمت مراجعته) __ //
        public int? ReviewedByTAId { get; set; }

        // __ Reference to the DiagnosisRecord (the case to be treated). __ //
        public int DiagnosisRecordId { get; set; }

        // __ Reference to the Student assigned to treat this case. __ //
        public int StudentId { get; set; }

        // __ Reference to the Term when this case is being handled. __ //
        public int TermId { get; set; }

        // __ Reference to the Clinic where the case will be handled. __ //
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public InternDoctor AssignedByIntern { get; set; } = null!;
        public TeachingAssistant? ReviewedByTA { get; set; }
        public DiagnosisRecord DiagnosisRecord { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
        public Term Term { get; set; } = null!;
        public ICollection<CaseSession> Sessions { get; set; } = new List<CaseSession>();
    }
}
