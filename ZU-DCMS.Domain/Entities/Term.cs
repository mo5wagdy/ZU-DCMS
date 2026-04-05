using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Term : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;
        public int RequiredCasesCount { get; set; }
        public string CreatedByAdminId { get; set; } = string.Empty;

        // _____________ Navigation _____________ //
        public ICollection<TermRequirement> TermRequirements { get; set; } = new List<TermRequirement>();
    }
}
