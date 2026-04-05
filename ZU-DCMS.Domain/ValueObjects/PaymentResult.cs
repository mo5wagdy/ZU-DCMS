using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.ValueObjects
{
    /*
     * This class represents the result of a payment operation, encapsulating information about whether the payment was successful,
     * any relevant codes or references, and error messages if applicable.
     */
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? PaymentCode { get; set; }
        public string? GatewayReference { get; set; }
        public string? GatewayName { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
