using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    // DTO for Student Progress information
    public class StudentProgressDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public int TotalRequired { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalTransferred { get; set; }
        public bool IsTermComplete { get; set; }
        public List<StudentRequirementDto> Requirements { get; set; } = new();

        // Computed property to calculate overall completion percentage
        public double OverallPercentage => TotalRequired == 0 ? 0 : Math.Round((double)(TotalCompleted + TotalTransferred) / TotalRequired * 100, 1);
    }
}
