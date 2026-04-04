using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Patient
{
    public class UpdatePatientDto
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int? Conditions { get; set; }
        public string? OtherConditions { get; set; }
    }
}
