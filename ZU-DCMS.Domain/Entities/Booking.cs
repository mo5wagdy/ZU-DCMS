using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;
using static System.Collections.Specialized.BitVector32;

namespace ZU_DCMS.Domain.Entities
{
    public class Booking : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string BookingCode { get; set; } = string.Empty;
        public BookingType BookingType { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? PreliminaryComplaint { get; set; }
        public string? PostponeReason { get; set; }
        public DateTime? PostponedAt { get; set; }

        // _____________ Foreign Keys _____________ //
        public int PatientId { get; set; }
        public int SessionId { get; set; }
        public int? PaymentId { get; set; }

        // _____________ Navigation _____________ //
        public Patient Patient { get; set; } = null!;
        public Session Session { get; set; } = null!;
        public Payment? Payment { get; set; }
        public DiagnosisRecord? DiagnosisRecord { get; set; }
    }
}
