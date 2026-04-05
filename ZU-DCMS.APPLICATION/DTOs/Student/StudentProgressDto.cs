using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    // __ DTO for Student Progress information __ //
    public class StudentProgressDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TotalRequired { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalTransferred { get; set; }
        public bool IsTermComplete { get; set; }
        public List<StudentRequirementDto> Requirements { get; set; } = new();

        // __ Computed property to calculate overall completion percentage __ //
        public double OverallPercentage => TotalRequired == 0 ? 0 : Math.Round((double)(TotalCompleted + TotalTransferred) / TotalRequired * 100, 1);
    }
}
