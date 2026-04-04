using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Session
{
    public class SessionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int MaxNewPatients { get; set; }
        public int MaxFollowUpPatients { get; set; }
        public int CurrentNewCount { get; set; }
        public int CurrentFollowUpCount { get; set; }
        public bool IsFull { get; set; }
    }
}
