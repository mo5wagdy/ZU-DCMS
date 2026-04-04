using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    // DTO for Student Priority information
    public class StudentPriorityDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public int CompletedCases { get; set; }
        public int RequiredCases { get; set; }
        public int Priority { get; set; }
        public bool IsComplete { get; set; }
    }
}
