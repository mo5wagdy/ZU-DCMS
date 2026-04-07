using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Patient
{
    // __ DTO for updating patient data __ //
    public class UpdatePatientDto
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string Address { get; set; } = null!;
        public ChronicCondition? ChronicConditions { get; set; }
        public string? OtherConditions { get; set; }
    }
}
