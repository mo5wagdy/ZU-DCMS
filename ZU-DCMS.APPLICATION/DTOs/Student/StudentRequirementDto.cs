using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    // DTO for Student Requirement information
    public class StudentRequirementDto
    {
        public int Id { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public int RequiredCount { get; set; }
        public int CompletedCount { get; set; }
        public int TransferredCount { get; set; }
        public bool IsSatisfied { get; set; }
        public int Priority { get; set; }

        // Computed property to calculate completion percentage
        public double CompletionPercentage => RequiredCount == 0 ? 0 : Math.Round((double)(CompletedCount + TransferredCount) / RequiredCount * 100, 1);
    }
}
