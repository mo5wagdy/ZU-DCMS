using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Booking
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string BookingType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PreliminaryComplaint { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionTime { get; set; } = string.Empty;
        public string? PaymentCode { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
