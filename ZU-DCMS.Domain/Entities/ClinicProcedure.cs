using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class ClinicProcedure : BaseEntity
    {
        public int ClinicId { get; set; }
        public int ProcedureId { get; set; }
        public bool IsActive { get; set; } = true;

        public Clinic Clinic { get; set; } = null!;
        public Procedure Procedure { get; set; } = null!;
    }
}
