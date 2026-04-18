namespace ZU_DCMS.APPLICATION.Features.Booking.Commands.CancelBooking
{
    public class CancelBookingCommand
    {
        public int BookingId { get; set; }
        public int PatientId { get; set; }
    }
}
