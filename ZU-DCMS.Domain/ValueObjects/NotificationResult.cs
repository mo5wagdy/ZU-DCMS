using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.ValueObjects
{
    // This class represents the result of a notification operation, encapsulating information about whether the notification was successful.
    public class NotificationResult
    {
        public bool IsSuccess { get; set; }
        public NotificationChannel Channel { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
