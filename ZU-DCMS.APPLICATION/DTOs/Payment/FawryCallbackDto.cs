
namespace ZU_DCMS.APPLICATION.DTOs.Payment
{
    /* 
     * DTO for Fawry payment callback information
     * This DTO is used to capture the details sent by Fawry when a payment is completed 
     */
    public class FawryCallbackDto
    {
        public string PaymentCode { get; set; } = string.Empty;
        public string GatewayReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string Signature { get; set; } = string.Empty; // => This is used to verify the authenticity of the callback from Fawry
    }
}
