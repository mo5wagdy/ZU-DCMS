using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class InternDoctor : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string ApplicationUserId { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // _____________ Navigation _____________ //
        public ICollection<DiagnosisRecord> DiagnosisRecords { get; set; } = new List<DiagnosisRecord>();
        public ICollection<CaseAssignment> CaseAssignments { get; set; } = new List<CaseAssignment>();
    }
}
