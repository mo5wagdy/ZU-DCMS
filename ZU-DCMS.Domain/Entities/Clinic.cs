
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Entities
{
        // __ Clinic entity representing a dental clinic specialization. __ //
    public class Clinic : BaseEntity
    {
        // _____________ Main Properties _____________ //
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int MaxDailyPatients { get; set; }

        // _____________ Academic Year Constraints _____________ //
        // __ Minimum academic year allowed for students in this clinic. __ //
        public int MinAcademicYear { get; set; } = 1;

        // __ Maximum academic year allowed for students in this clinic. __ //
        public int MaxAcademicYear { get; set; } = 4;

        // _____________ Workload Constraints _____________ //
        // __ Maximum number of active (InProgress) cases a student can have in this clinic simultaneously. __ //
        public int MaxCasesPerStudent { get; set; } = 3;

        // _____________ Navigation _____________ //
        public ICollection<DiagnosisRecord> DiagnosisRecords { get; set; } = new List<DiagnosisRecord>();
        public ICollection<CaseAssignment> CaseAssignments { get; set; } = new List<CaseAssignment>();
        public ICollection<TermRequirement> TermRequirements { get; set; } = new List<TermRequirement>();
        public ICollection<ClinicDiagnosisType> DiagnosisTypeLinks { get; set; } = new List<ClinicDiagnosisType>();
        public ICollection<ClinicProcedure> ProcedureLinks { get; set; } = new List<ClinicProcedure>();
        public ICollection<CaseSession> CaseSessions { get; set; } = new List<CaseSession>();
    }
}
