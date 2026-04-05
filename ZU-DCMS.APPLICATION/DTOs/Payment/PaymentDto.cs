using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Payment
{
    // DTO for Payment information
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; }
        public string? PaymentCode { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
