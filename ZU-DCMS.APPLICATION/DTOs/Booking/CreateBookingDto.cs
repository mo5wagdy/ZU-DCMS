using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Booking
{
    // __ DTO for creating a new booking __ //
    public class CreateBookingDto
    {
        public BookingType BookingType { get; set; }
        public DateTime PreferredDate { get; set; }
        public string PreferredTimeSlot { get; set; } = null!;
        public string? PreliminaryComplaint { get; set; }
    }
}
