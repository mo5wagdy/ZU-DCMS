using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Enums
{
    // This enum represents the status of a notification, indicating whether it is pending, sent, or failed.
    public enum NotificationStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3
    }
}
