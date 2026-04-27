using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    public class DiagnosisType : BaseEntity
    {
        // __ Represents a type of diagnosis that can be associated with a clinic __ //
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<ClinicDiagnosisType> ClinicLinks { get; set; } = new List<ClinicDiagnosisType>();
        public ICollection<DiagnosisRecord> DiagnosisRecords { get; set; } = new List<DiagnosisRecord>();
    }
}
