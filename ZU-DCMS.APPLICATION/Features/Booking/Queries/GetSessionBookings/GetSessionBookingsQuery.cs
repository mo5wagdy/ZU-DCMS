using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Booking.Queries.GetSessionBookings
{
    public class GetSessionBookingsQuery
    {
        public int SessionId { get; set; }
        public PagedRequest Request { get; set; } = null!;
    }
}
