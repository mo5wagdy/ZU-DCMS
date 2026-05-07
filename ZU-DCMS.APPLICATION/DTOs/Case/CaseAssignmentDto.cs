
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // __ DTO for Case Assignment information __ //
    public class CaseAssignmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;

        // __ Clinic (bilingual) __ //
        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;    // Arabic (legacy)
        public string ClinicNameEn { get; set; } = string.Empty;  // English

        // __ Diagnosis (bilingual) __ //
        public string Diagnosis { get; set; } = string.Empty;     // Arabic
        public string DiagnosisEn { get; set; } = string.Empty;   // English

        // __ Student info __ //
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;

        // __ Intern info __ //
        public string AssignedByInternName { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public CaseStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? AssignmentReviewedAt { get; set; }

        // __ Session log __ //
        public List<CaseSessionDto> Sessions { get; set; } = new();
    }
}
