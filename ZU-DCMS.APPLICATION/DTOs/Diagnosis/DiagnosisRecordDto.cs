using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // __ Dto for returning diagnosis record details. __ //
    public class DiagnosisRecordDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string InternDoctorName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string Complaint { get; set; } = string.Empty;
        public string DiagnosisTypeName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime DiagnosedAt { get; set; }
        public bool IsAssigned { get; set; }
    }

}
