using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Booking
{
    // __ DTO for transferring booking data __ //
    public class BookingDto
    {
        public int Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public BookingType BookingType { get; set; }
        public BookingStatus Status { get; set; }
        public string? PreliminaryComplaint { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionTime { get; set; } = string.Empty;
    }
}
