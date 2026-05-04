using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // __ Dto for booking details to be used in diagnosis process __ //
    public class BookingForDiagnosisDto
    {
        public int BookingId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int PatientAge { get; set; }
        public string PatientGender { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public ChronicCondition Conditions { get; set; }
        public string? OtherConditions { get; set; }
        public string? PreliminaryComplaint { get; set; }
        public bool IsDiagnosed { get; set; }
        public bool IsAssigned { get; set; }
        public BookingStatus Status { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }
    }
}
