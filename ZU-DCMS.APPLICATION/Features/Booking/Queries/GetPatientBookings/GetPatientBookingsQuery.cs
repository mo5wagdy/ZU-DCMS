using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Booking.Queries.GetPatientBookings
{
    public class GetPatientBookingsQuery
    {
        public int PatientId { get; set; }
        public PagedRequest Request { get; set; } = null!;
    }
}
