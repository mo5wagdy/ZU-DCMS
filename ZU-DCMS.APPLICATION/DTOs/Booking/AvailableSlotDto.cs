using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Booking
{
    public class AvailableSlotDto
    {
        public int SessionId { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int AvailableNewSlots { get; set; }
        public int AvailableFollowUpSlots { get; set; }
        public bool IsAvailable { get; set; }
    }
}
