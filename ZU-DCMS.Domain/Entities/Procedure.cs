
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Procedure : BaseEntity
    {
        // __ Represents a medical procedure that can be performed during a case session __ //
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<ClinicProcedure> ClinicLinks { get; set; } = new List<ClinicProcedure>();
        public ICollection<CaseSessionProcedure> CaseSessionProcedures { get; set; } = new List<CaseSessionProcedure>();
    }
}
