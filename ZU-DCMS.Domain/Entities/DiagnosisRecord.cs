using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    /// <summary>
    /// DiagnosisRecord represents the diagnosis made by an InternDoctor for a patient's booking.
    /// 
    /// Key Point for Case Assignment:
    /// This entity links the diagnosis to a specific Clinic through:
    /// 1. DiagnosisType → has ClinicId
    /// 2. ClinicId → stored directly for quick validation
    /// 
    /// The ClinicId stored here is used by AssignStudentHandler to auto-populate
    /// the clinic context for student eligibility validation.
    /// </summary>
    public class DiagnosisRecord : BaseEntity
    {
        // _____________ Main Properties _____________ //
        /// <summary>
        /// The patient's complaint or chief complaint recorded during diagnosis.
        /// </summary>
        public string Complaint { get; set; } = string.Empty;

        /// <summary>
        /// Additional diagnostic notes or observations.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Timestamp when the diagnosis was made.
        /// </summary>
        public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Flag indicating whether a student has been assigned to this case.
        /// Used to prevent multiple assignments to the same case.
        /// 
        /// Workflow:
        /// - IsAssigned = false: Case available for assignment
        /// - IsAssigned = true: Case has a CaseAssignment; mark as not available
        /// </summary>
        public bool IsAssigned { get; set; } = false;

        // _____________ Foreign Keys _____________ //
        /// <summary>
        /// Reference to the DiagnosisType (e.g., Cavity, Extraction, Root Canal).
        /// Links to a specific clinic through DiagnosisType.ClinicId.
        /// </summary>
        public int DiagnosisTypeId { get; set; }

        /// <summary>
        /// Reference to the original Booking that triggered this diagnosis.
        /// Maintains link to patient and appointment information.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// The InternDoctor who made the diagnosis.
        /// </summary>
        public int InternDoctorId { get; set; }

        /// <summary>
        /// Reference to the Clinic where the diagnosis was made.
        /// CRITICAL for case assignment: This clinic ID must be used to validate
        /// student's academic year range and workload in THIS specific clinic.
        /// </summary>
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public InternDoctor InternDoctor { get; set; } = null!;
        public DiagnosisType DiagnosisType { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;

        /// <summary>
        /// The CaseAssignment created from this diagnosis (one-to-one relationship).
        /// Null if not yet assigned to a student.
        /// </summary>
        public CaseAssignment? CaseAssignment { get; set; }
    }
}
