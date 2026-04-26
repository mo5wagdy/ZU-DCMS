using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    // __ DTO for Student Requirement information __ //
    public class StudentRequirementDto
    {
        public int Id { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public string RequirementTypeName { get; set; } = string.Empty;
        public int RequiredCount { get; set; }
        public int CompletedCount { get; set; }
        public int TransferredCount { get; set; }
        public bool IsSatisfied { get; set; }
        public int Priority { get; set; }

        // __ Computed property to calculate completion percentage __ //
        public double CompletionPercentage => RequiredCount == 0 ? 0 : Math.Round((double)(CompletedCount + TransferredCount) / RequiredCount * 100, 1);
    }
}
