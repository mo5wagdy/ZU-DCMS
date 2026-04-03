using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class Payment : BaseEntity
    {
        // Properties
        public int PatientId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaymentCode { get; set; }
        public string? GatewayReference { get; set; }
        public string? GatewayName { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // Navigation
        public Patient Patient { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }
}
