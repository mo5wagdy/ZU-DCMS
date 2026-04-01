using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.ValueObjects
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? FawryCode { get; set; }
        public string? Reference { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
