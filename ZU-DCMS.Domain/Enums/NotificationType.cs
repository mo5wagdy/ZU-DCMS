using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Enums
{
    // This enum represents the different types of notifications that can be sent to users,
    // such as booking confirmations, student assignments, case completions, case transfers, and appointment postponements.
    public enum NotificationType
    {
        BookingConfirmed = 1,
        StudentAssigned = 2,
        CaseCompleted = 3,
        CaseTransferred = 4,
        AppointmentPostponed = 5
    }
}
