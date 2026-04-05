using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Patient
{
    // __ DTO for transferring patient data __ //
    public class PatientDto
    {
        public int Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IdentityType IdentityType { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string NationalityCode { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public ChronicCondition ChronicConditions { get; set; }
        public string? OtherConditions { get; set; }
        public bool IsActive { get; set; }
    }
}
