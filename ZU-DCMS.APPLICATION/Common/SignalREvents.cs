using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    // __ Centralized class for SignalR event names to ensure consistency across the application and avoid hardcoding strings in multiple places. __ //
    public static class SignalREvents
    {
        // ________ Dashboard Events ________ //
        public const string DashboardUpdated = "dashboard_updated";

        // ________ Session Events ________ //
        public const string SessionUpdated = "session_updated";

        // ________ Booking Events ________ //
        public const string BookingCreated = "booking_created";
        public const string BookingCancelled = "booking_cancelled";

        // ________ Case Events ________ //
        public const string CaseAssigned = "case_assigned";
        public const string CaseCompleted = "case_completed";
    }
}
