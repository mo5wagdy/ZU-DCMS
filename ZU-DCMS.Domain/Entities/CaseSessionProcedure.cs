using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class CaseSessionProcedure : BaseEntity
    {
        // __ Represents the association between a case session and a procedure performed during that session __ //
        public int CaseSessionId { get; set; }
        public int ProcedureId { get; set; }

        // __ Navigation properties __ //
        public CaseSession Session { get; set; } = null!;
        public Procedure Procedure { get; set; } = null!;
    }
}
