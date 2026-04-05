using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Dashboard
{
    // __ DTO for Dashboard information __ //
    public class DashboardDto
    {
        public int TotalBookingsToday { get; set; }
        public int NewPatientsToday { get; set; }
        public int FollowUpPatientsToday { get; set; }
        public int CancelledToday { get; set; }
        public int ActiveCases { get; set; }
        public int CompletedCasesToday { get; set; }
        public List<ClinicStatusDto> ClinicsStatus { get; set; } = new();
        public List<SessionStatusDto> SessionsStatus { get; set; } = new();
    }
}
