using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class ClinicDiagnosisType : BaseEntity
    {
        public int ClinicId { get; set; }
        public int DiagnosisTypeId { get; set; }
        public bool IsActive { get; set; } = true;

        public Clinic Clinic { get; set; } = null!;
        public DiagnosisType DiagnosisType { get; set; } = null!;
    }
}
