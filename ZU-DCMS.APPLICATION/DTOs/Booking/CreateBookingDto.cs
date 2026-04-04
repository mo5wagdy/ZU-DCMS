using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Booking
{
    public class CreateBookingDto
    {
        public string BookingType { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string? PreliminaryComplaint { get; set; }
    }
}
