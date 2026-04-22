using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    /// <summary>
    /// DTO for Student Priority information used in case assignment queries.
    /// 
    /// This DTO is returned by GetAvailableStudentsHandler and includes:
    /// - Student identification (code, name, academic year)
    /// - Progress toward requirement (completed vs required cases)
    /// - Priority ranking (determined by AI or fallback)
    /// - Completion status
    /// </summary>
    public class StudentPriorityDto
    {
        /// <summary>
        /// Unique identifier for the student.
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Student's code (e.g., "STU-2024-001").
        /// </summary>
        public string StudentCode { get; set; } = string.Empty;

        /// <summary>
        /// Student's full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Student's current academic year (1=first year, 4=fourth year).
        /// Used for validation and filtering in UI.
        /// </summary>
        public int AcademicYear { get; set; }

        /// <summary>
        /// Number of cases already completed in this clinic for this term.
        /// </summary>
        public int CompletedCases { get; set; }

        /// <summary>
        /// Total number of cases required in this clinic for this term.
        /// </summary>
        public int RequiredCases { get; set; }

        /// <summary>
        /// Priority rank for case assignment (1 = highest priority / first choice).
        /// Determined by AI agent prioritization or fallback algorithm.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Indicates whether the student has already completed all required cases.
        /// Should typically be false for students in this list (unsatisfied requirements).
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Internal requirement priority (1-4) for reference.
        /// Lower number = higher need for cases.
        /// </summary>
        public int RequirementPriority { get; set; }
    }
}
