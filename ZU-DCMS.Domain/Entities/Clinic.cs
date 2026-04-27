using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
    /// <summary>
    /// Clinic entity representing a dental clinic specialization.
    /// 
    /// Academic Year Constraints:
    /// Clinics are restricted to specific academic year ranges (e.g., Clinic A accepts students from 2nd to 4th year).
    /// This ensures proper student progression and skill-level alignment with clinic complexity.
    /// </summary>
    public class Clinic : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int MaxDailyPatients { get; set; }

        // _____________ Academic Year Constraints _____________ //
        /// <summary>
        /// Minimum academic year allowed for students in this clinic.
        /// Example: 2 means only 2nd year and above can be assigned to this clinic.
        /// </summary>
        public int MinAcademicYear { get; set; } = 1;

        /// <summary>
        /// Maximum academic year allowed for students in this clinic.
        /// Example: 4 means only up to 4th year students can be assigned to this clinic.
        /// </summary>
        public int MaxAcademicYear { get; set; } = 4;

        // _____________ Workload Constraints _____________ //
        /// <summary>
        /// Maximum number of active (InProgress) cases a student can have in this clinic simultaneously.
        /// Prevents student overload and ensures case quality.
        /// Default: 3 cases maximum per student per clinic.
        /// </summary>
        public int MaxCasesPerStudent { get; set; } = 3;

        // _____________ Navigation _____________ //
        public ICollection<DiagnosisRecord> DiagnosisRecords { get; set; } = new List<DiagnosisRecord>();
        public ICollection<CaseAssignment> CaseAssignments { get; set; } = new List<CaseAssignment>();
        public ICollection<TermRequirement> TermRequirements { get; set; } = new List<TermRequirement>();
        public ICollection<DiagnosisType> DiagnosisTypes { get; set; } = new List<DiagnosisType>();
        public ICollection<ClinicDiagnosisType> DiagnosisTypeLinks { get; set; } = new List<ClinicDiagnosisType>();
        public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
        public ICollection<ClinicProcedure> ProcedureLinks { get; set; } = new List<ClinicProcedure>();
        public ICollection<CaseSession> CaseSessions { get; set; } = new List<CaseSession>();
    }
}
