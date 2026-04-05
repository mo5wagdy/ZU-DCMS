using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    // __ Dto for assigning a student to a diagnosis record __ //
    public class AssignStudentDto
    {
        public int DiagnosisRecordId { get; set; }
        public int StudentId { get; set; }
        public int ClinicId { get; set; }
        public string? Notes { get; set; }
    }
}
