using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Dashboard
{
    // DTO for Session status on the Dashboard
    public class SessionStatusDto
    {
        public int SessionId { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public int CurrentNewCount { get; set; }
        public int MaxNewPatients { get; set; }
        public int CurrentFollowUpCount { get; set; }
        public int MaxFollowUpPatients { get; set; }
        public bool IsFull { get; set; }
    }
}
