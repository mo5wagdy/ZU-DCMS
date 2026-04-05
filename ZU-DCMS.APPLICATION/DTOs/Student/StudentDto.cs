using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Student
{
    // __ DTO for Student entity __ //
    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public bool IsActive { get; set; }
    }
}
