namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for transferring daily dashboard metrics __ //
    public class DailyMetricsDto
    {
        // __ Patient Metrics __ //
        public int TodayNewPatientsCount { get; set; }

        // __ Booking Metrics __ //
        public int TodayBookingsCount { get; set; }
        public int TodayNewBookingsCount { get; set; }
        public int TodayFollowUpBookingsCount { get; set; }
        public int PendingBookingsCount { get; set; }
        public int CancelledBookingsCount { get; set; }

        // __ Case Metrics __ //
        public int InProgressCasesCount { get; set; }
        public int CompletedCasesCount { get; set; }

        // __ General Metrics __ //
        public int ActiveSessionsCount { get; set; }
        public int TotalActiveStudents { get; set; }
    }
}
