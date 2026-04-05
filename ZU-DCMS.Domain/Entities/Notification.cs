using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.Domain.Entities
{
    public class Notification : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public string? ReferenceId { get; set; }
        public ReferenceType? ReferenceType { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
