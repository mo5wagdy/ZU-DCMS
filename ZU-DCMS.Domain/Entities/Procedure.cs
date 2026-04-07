using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Procedure : BaseEntity
    {
        // __ Represents a medical procedure that can be performed during a case session __ //
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int ClinicId { get; set; }
        public bool IsActive { get; set; } = true;

        // __ Navigation property to the associated clinic __ //
        public Clinic Clinic { get; set; } = null!;
        public ICollection<CaseSessionProcedure> CaseSessionProcedures { get; set; } = new List<CaseSessionProcedure>();
    }
}
