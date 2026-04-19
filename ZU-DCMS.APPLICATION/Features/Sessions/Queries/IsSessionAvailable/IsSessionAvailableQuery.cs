using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Features.Session.Queries.IsSessionAvailable
{
    public class IsSessionAvailableQuery
    {
        public int SessionId { get; set; }
        public BookingType BookingType { get; set; }
    }
}
