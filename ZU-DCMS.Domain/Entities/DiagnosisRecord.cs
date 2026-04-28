using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
        // __ DiagnosisRecord represents the diagnosis made by an InternDoctor for a patient's booking. __ //
    public class DiagnosisRecord : BaseEntity
    {
        // _____________ Main Properties _____________ //
        // __ The patient's complaint or chief complaint recorded during diagnosis. __ //
        public string Complaint { get; set; } = string.Empty;

        // __ Additional diagnostic notes or observations. __ //
        public string? Notes { get; set; }

        // __ Timestamp when the diagnosis was made. __ //
        public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;

        // __ Flag indicating whether a student has been assigned to this case. __ //
        public bool IsAssigned { get; set; } = false;

        // _____________ Foreign Keys _____________ //
        // __ Reference to the DiagnosisType (e.g., Cavity, Extraction, Root Canal). __ //
        public int DiagnosisTypeId { get; set; }

        // __ Reference to the original Booking that triggered this diagnosis. __ //
        public int BookingId { get; set; }

        // __ The InternDoctor who made the diagnosis. __ //
        public int InternDoctorId { get; set; }

        // __ Reference to the Clinic where the diagnosis was made. __ //
        public int ClinicId { get; set; }

        // _____________ Navigation _____________ //
        public InternDoctor InternDoctor { get; set; } = null!;
        public DiagnosisType DiagnosisType { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;

        // __ The CaseAssignment created from this diagnosis (one-to-one relationship). __ //
        public CaseAssignment? CaseAssignment { get; set; }
    }
}
