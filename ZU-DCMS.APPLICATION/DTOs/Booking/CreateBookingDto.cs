using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Booking
{
    // __ DTO for creating a new booking __ //
    public class CreateBookingDto
    {
        public BookingType BookingType { get; set; }
        public int SessionId { get; set; }
        public string? PreliminaryComplaint { get; set; }
    }
}
