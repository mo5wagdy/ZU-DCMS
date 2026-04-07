using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // __ Dto for creating a new diagnosis __ //
    public class CreateDiagnosisDto
    {
        public int BookingId { get; set; }
        public int ClinicId { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public int DiagnosisTypeId { get; set; }
        public string? Notes { get; set; }
    }
}
