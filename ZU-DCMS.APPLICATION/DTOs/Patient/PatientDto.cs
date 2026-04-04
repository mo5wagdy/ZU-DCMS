using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Patient
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string IdentityType { get; set; } = string.Empty;
        public string NationalityCode { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
        public string? OtherConditions { get; set; }
        public bool IsActive { get; set; }
    }
}
