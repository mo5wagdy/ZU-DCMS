using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Session : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxNewPatients { get; set; }
        public int MaxFollowUpPatients { get; set; }
        public int CurrentNewCount { get; set; }
        public int CurrentFollowUpCount { get; set; }
        public bool IsActive { get; set; } = true;

        // _____________ Calculated properties _____________ //
        /* __ These properties are not mapped to the database but calculated at runtime,
        They can be used to easily check if the session is full for new or follow-up patients __ */
        public bool IsNewFull => CurrentNewCount >= MaxNewPatients;
        public bool IsFollowUpFull => CurrentFollowUpCount >= MaxFollowUpPatients;
        public bool IsFull => IsNewFull && IsFollowUpFull;

        // _____________ Navigation _____________ //
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
