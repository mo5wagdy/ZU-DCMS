using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    public class BookingForDiagnosisDto
    {
        public int BookingId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int PatientAge { get; set; }
        public string PatientGender { get; set; } = string.Empty;
        public string? PreliminaryComplaint { get; set; }
        public string Conditions { get; set; } = string.Empty;
        public string? OtherConditions { get; set; }
        public bool IsAssigned { get; set; }
    }
}
