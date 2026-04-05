using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class Payment : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaymentCode { get; set; }
        public string? GatewayReference { get; set; }
        public string? GatewayName { get; set; }
        public DateTime? PaidAt { get; set; }

        // _____________ Foreign Keys _____________ //
        public int PatientId { get; set; }
        public int BookingId { get; set; }

        // _____________ Navigation _____________ //
        public Patient Patient { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }
}
