
namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // __ Dto for returning diagnosis record details. __ //
    public class DiagnosisRecordDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string InternDoctorName { get; set; } = string.Empty;

        // __ Clinic (bilingual) __ //
        public string ClinicName { get; set; } = string.Empty;      // Arabic (legacy)
        public string ClinicNameEn { get; set; } = string.Empty;    // English

        // __ Diagnosis Type (bilingual) __ //
        public string DiagnosisTypeName { get; set; } = string.Empty;      // Arabic
        public string DiagnosisTypeNameEn { get; set; } = string.Empty;    // English

        public int ClinicId { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime DiagnosedAt { get; set; }
        public bool IsAssigned { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }
    }
}
