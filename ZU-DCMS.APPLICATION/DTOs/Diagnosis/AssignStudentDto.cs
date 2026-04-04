using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // Dto for assigning a student to a diagnosis record
    public class AssignStudentDto
    {
        public int DiagnosisRecordId { get; set; }
        public int StudentId { get; set; }
        public int ClinicId { get; set; }
        public string? Notes { get; set; }
    }
}
