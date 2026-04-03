using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class Patient : BaseEntity
    {
        // Properties
        public string ApplicationUserId { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public IdentityType IdentityType { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string NationalityCode { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public ChronicCondition ChronicConditions { get; set; } = ChronicCondition.None;
        public string? OtherConditions { get; set; }

        // Navigation
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
