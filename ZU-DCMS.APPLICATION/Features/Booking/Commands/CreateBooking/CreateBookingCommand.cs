using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Features.Booking.Commands.CreateBooking
{
    public class CreateBookingCommand
    {
        public int PatientId { get; set; }
        public CreateBookingDto Dto { get; set; } = null!;
    }
}
