using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class Clinic : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int MaxDailyPatients { get; set; }

        // _____________ Navigation _____________ //
        public ICollection<DiagnosisRecord> DiagnosisRecords { get; set; } = new List<DiagnosisRecord>();
        public ICollection<CaseAssignment> CaseAssignments { get; set; } = new List<CaseAssignment>();
        public ICollection<TermRequirement> TermRequirements { get; set; } = new List<TermRequirement>();
        public ICollection<DiagnosisType> DiagnosisTypes { get; set; } = new List<DiagnosisType>();
        public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
    }
}
