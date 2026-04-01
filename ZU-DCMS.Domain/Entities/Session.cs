using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Session : BaseEntity
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxNewPatients { get; set; } = 25;
        public int MaxFollowUpPatients { get; set; } = 25;
        public int CurrentNewCount { get; set; }
        public int CurrentFollowUpCount { get; set; }
        public bool IsActive { get; set; } = true;

        // Calculated
        public bool IsNewFull => CurrentNewCount >= MaxNewPatients;
        public bool IsFollowUpFull => CurrentFollowUpCount >= MaxFollowUpPatients;
        public bool IsFull => IsNewFull && IsFollowUpFull;

        // Navigation
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
