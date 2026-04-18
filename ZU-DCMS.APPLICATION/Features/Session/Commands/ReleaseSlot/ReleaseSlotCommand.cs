using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Features.Session.Commands.ReleaseSlot
{
    public class ReleaseSlotCommand
    {
        public int SessionId { get; set; }
        public BookingType Type { get; set; }
    }
}
