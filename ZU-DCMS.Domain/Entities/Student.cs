using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Student : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string ApplicationUserId { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        // string Email { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public int? ActiveTermId { get; set; }
        public bool IsActive { get; set; } = true;

        // _____________ Navigation _____________ //
        public Term? ActiveTerm { get; set; }
        public ICollection<TermRequirement> TermRequirements { get; set; } = new List<TermRequirement>();
        public ICollection<CaseAssignment> CaseAssignments { get; set; } = new List<CaseAssignment>();
    }
}
