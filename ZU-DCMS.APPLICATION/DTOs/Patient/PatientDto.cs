using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Patient
{
    // __ Result wrapper for patient operations, encapsulating success status, error messages, and patient data __ //
    public class PatientResult : Result<PatientDto>
    {
        private PatientResult(PatientDto value, bool isSuccess, string error) : base(value, isSuccess, error) { }
        public static PatientResult Success(PatientDto data) => new(data, true, string.Empty);
        public static PatientResult Fail(string error) => new(default!, false, error);
    }

    // __ DTO for transferring patient data __ //
    public class PatientDto
    {
        public int Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IdentityType IdentityType { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public ChronicCondition ChronicConditions { get; set; }
        public string? OtherConditions { get; set; }
        public bool IsActive { get; set; }
    }
}
