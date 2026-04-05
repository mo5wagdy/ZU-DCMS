using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Dashboard
{
    // __ DTO for Clinic status on the Dashboard __ //
    public class ClinicStatusDto
    {
        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public int ActiveCases { get; set; }
        public int CompletedToday { get; set; }
        public int AvailableStudents { get; set; }
    }
}
