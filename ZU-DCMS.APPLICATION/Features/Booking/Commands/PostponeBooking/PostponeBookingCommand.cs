namespace ZU_DCMS.APPLICATION.Features.Booking.Commands.PostponeBooking
{
    public class PostponeBookingCommand
    {
        public int BookingId { get; set; }
        public string Reason { get; set; } = null!;
        public string AdminId { get; set; } = null!;
    }
}
