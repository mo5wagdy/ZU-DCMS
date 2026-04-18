using ZU_DCMS.APPLICATION.DTOs.Payment;

namespace ZU_DCMS.APPLICATION.Features.FawrySignature.Queries.ValidateSignature
{
    public class ValidateSignatureQuery
    {
        public FawryCallbackDto Dto { get; set; } = null!;
    }
}
