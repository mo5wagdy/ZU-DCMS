
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.DTOs.Case
{
    // __ DTO for Case Assignment information __ //
    public class CaseAssignmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public string AssignedByInternName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public CaseStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public List<CaseSessionDto> Sessions { get; set; } = new();
    }
}
