using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Payment
{
    // DTO for Fawry payment callback information
    // This DTO is used to capture the details sent by Fawry when a payment is completed
    public class FawryCallbackDto
    {
        public string PaymentCode { get; set; } = string.Empty;
        public string GatewayReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
