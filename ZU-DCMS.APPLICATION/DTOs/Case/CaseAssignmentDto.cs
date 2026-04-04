using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // DTO for Case Assignment information
    public class CaseAssignmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string AssignedByInternName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public List<CaseSessionDto> Sessions { get; set; } = new();
    }
}
