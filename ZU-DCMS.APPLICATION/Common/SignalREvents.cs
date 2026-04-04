using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    // Centralized class for SignalR event names to ensure consistency across the application
    public static class SignalREvents
    {
        // Dashboard Events
        public const string DashboardUpdated = "dashboard_updated";

        // Session Events
        public const string SessionUpdated = "session_updated";

        // Booking Events
        public const string BookingCreated = "booking_created";
        public const string BookingCancelled = "booking_cancelled";

        // Case Events
        public const string CaseAssigned = "case_assigned";
        public const string CaseCompleted = "case_completed";
    }
}
