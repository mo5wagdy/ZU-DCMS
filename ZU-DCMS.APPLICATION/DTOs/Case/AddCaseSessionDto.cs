using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // __ DTO for adding a new Case Session __ //
    public class AddCaseSessionDto
    {
        public int CaseAssignmentId { get; set; }
        public string ProceduresDone { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string? Notes { get; set; }
    }
}
