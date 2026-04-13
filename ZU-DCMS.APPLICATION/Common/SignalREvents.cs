using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    // __ Centralized class for SignalR event names to ensure consistency across the application and avoid hardcoding strings in multiple places. __ //
    public static class SignalREvents
    {
        // ________ Dashboard Events ________ //
        public const string StatsUpdated = "stats_updated";

        // ________ Session Events ________ //
        public const string SessionUpdated = "session_updated";
        public const string SessionFull = "session_full";

        // ________ Booking Events ________ //
        public const string BookingCreated = "booking_created";
        public const string BookingCancelled = "booking_cancelled";
        public const string BookingPostponed = "booking_postponed";

        // ________ Payment Events ________ //
        public const string PaymentPaid = "payment_paid";

        // ________ Diagnosis Events ________ //
        public const string DiagnosisCreated = "diagnosis_created";

        // ________ Case Events ________ //
        public const string CaseAssigned = "case_assigned";
        public const string CaseCompleted = "case_completed";
        public const string CaseHasFollowUp = "case_has_followup";

        // ________ Alerts ________ //
        public const string LowStudentsAlert = "low_students_alert";
    }
}
