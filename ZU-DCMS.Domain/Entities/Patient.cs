using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class Patient : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string ApplicationUserId { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IdentityType IdentityType { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateOfBirth.Date > DateTime.Today.AddYears(-(DateTime.Today.Year - DateOfBirth.Year)) ? 1 : 0); // => Computed property to calculate age based on DateOfBirth.
        public Gender Gender { get; set; }
        public ChronicCondition ChronicConditions { get; set; } = ChronicCondition.None;
        public string? OtherConditions { get; set; }

        // _____________ Navigation _____________ //
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
