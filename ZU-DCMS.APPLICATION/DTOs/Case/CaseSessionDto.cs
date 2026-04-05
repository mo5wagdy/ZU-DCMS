using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // __ DTO for Case Session information __ //
    public class CaseSessionDto
    {
        public int Id { get; set; }
        public string ProceduresDone { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string? Notes { get; set; }
        public DateTime SessionDate { get; set; }
    }
}
