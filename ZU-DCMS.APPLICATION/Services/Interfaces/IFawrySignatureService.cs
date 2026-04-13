
using ZU_DCMS.APPLICATION.DTOs.Payment;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    public interface IFawrySignatureService
    {
        bool IsValid(FawryCallbackDto dto);
    }
}
