using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // Dto for creating a new diagnosis
    public class CreateDiagnosisDto
    {
        public int BookingId { get; set; }
        public int ClinicId { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
