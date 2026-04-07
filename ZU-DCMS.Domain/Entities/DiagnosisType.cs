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
        public int ClinicId { get; set; }  
        public bool IsActive { get; set; } = true;

        // __ Navigation __ //
        public Clinic Clinic { get; set; } = null!;
        public ICollection<DiagnosisRecord> DiagnosisRecords { get; set; } = new List<DiagnosisRecord>();
    }
}
