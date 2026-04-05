using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Enums
{
    // __ This enum represents the different types of references that can be associated with notifications or payments in the system such as bookings, cases, or payments. __ //
    public enum ReferenceType
    {
        Booking = 1,
        Case = 2,
        Payment = 3
    }
}
