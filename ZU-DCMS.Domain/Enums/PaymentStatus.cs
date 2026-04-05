using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Enums
{
    // __ This enum represents the status of a payment, indicating whether it is pending, paid, failed, or refunded. __ //
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Cancelled = 4
    }
}
