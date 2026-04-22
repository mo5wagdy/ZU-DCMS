
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    /// <summary>
    /// CaseAssignment represents the assignment of a diagnosed patient case to a student.
    /// 
    /// Key Responsibilities:
    /// - Links a diagnosis to a specific student for treatment
    /// - Tracks case completion status and sessions
    /// - Maintains data integrity by storing TermId, ClinicId, and StudentId atomically
    /// - Enables academic year and clinic-specific validations
    /// 
    /// Workflow:
    /// 1. Doctor diagnoses a patient (creates DiagnosisRecord)
    /// 2. System or doctor assigns case to eligible student (creates CaseAssignment)
    /// 3. Student attends sessions (creates CaseSession records)
    /// 4. Case is marked as Completed or Transferred
    /// 5. TermRequirement is updated to reflect progress
    /// </summary>
    public class CaseAssignment : BaseEntity
    {
        // _____________ Main Properties _____________ //
        /// <summary>
        /// Current status of the case assignment.
        /// 
        /// Status Flow:
        /// - Active: Case just assigned, student hasn't started sessions
        /// - InProgress: Student has started working on the case
        /// - Completed: Case work is finished, meets all requirements
        /// - Transferred: Case transferred to another student or clinic
        /// - Cancelled: Assignment was cancelled
        /// </summary>
        public CaseStatus Status { get; set; } = CaseStatus.Active;

        /// <summary>
        /// Timestamp when this case was assigned to the student.
        /// Used for audit trail and workload distribution analysis.
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional notes about the assignment (e.g., special instructions, constraints).
        /// </summary>
        public string? Notes { get; set; }

        // _____________ Foreign Keys _____________ //
        /// <summary>
        /// The InternDoctor who assigned this case.
        /// Required for audit trail and permission validation.
        /// </summary>
        public int AssignedByInternId { get; set; }

        /// <summary>
        /// Reference to the DiagnosisRecord (the case to be treated).
        /// Links the assignment to the original diagnosis.
        /// </summary>
        public int DiagnosisRecordId { get; set; }

        /// <summary>
        /// Reference to the Student assigned to treat this case.
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Reference to the Term when this case is being handled.
        /// CRITICAL: Must come from student's ActiveTermId at the time of assignment.
        /// Used for tracking progress against TermRequirement.
        /// </summary>
        public int TermId { get; set; }

        /// <summary>
        /// Reference to the Clinic where the case will be handled.
        /// CRITICAL: Auto-retrieved from DiagnosisType's ClinicId.
        /// Used for validating student's academic year against clinic constraints.
        /// </summary>
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public InternDoctor AssignedByIntern { get; set; } = null!;
        public DiagnosisRecord DiagnosisRecord { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
        public Term Term { get; set; } = null!;
        public ICollection<CaseSession> Sessions { get; set; } = new List<CaseSession>();
    }
}
