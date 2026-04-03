using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Enums
{
    // This enum represents the various statuses that a booking can have in the system.
    public enum BookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Postponed = 4,
        Completed = 5
    }
}
