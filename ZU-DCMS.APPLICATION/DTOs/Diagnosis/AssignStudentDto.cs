
namespace ZU_DCMS.APPLICATION.DTOs.Diagnosis
{
    /// <summary>
    /// DTO for assigning a student to a diagnosed case (DiagnosisRecord).
    /// 
    /// NOTE: ClinicId is NOT included in this DTO.
    /// 
    /// Why ClinicId is Auto-Populated:
    /// The ClinicId is automatically retrieved from the DiagnosisRecord's ClinicId field.
    /// This ensures data integrity and prevents inconsistencies where a student is assigned
    /// to a case in a different clinic than the one where the diagnosis was made.
    /// 
    /// Data Flow:
    /// 1. Client sends: DiagnosisRecordId + StudentId + (optional) Notes
    /// 2. Handler retrieves DiagnosisRecord and extracts its ClinicId
    /// 3. Handler validates student against Clinic's academic year range and workload
    /// 4. CaseAssignment is created with auto-populated ClinicId
    /// </summary>
    public class AssignStudentDto
    {
        /// <summary>
        /// ID of the DiagnosisRecord (the case to be assigned).
        /// </summary>
        public int DiagnosisRecordId { get; set; }

        /// <summary>
        /// ID of the Student to assign to this case.
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Optional notes or instructions for the case assignment.
        /// </summary>
        public string? Notes { get; set; }

        // ⚠️ IMPORTANT: DO NOT include ClinicId here.
        // ClinicId will be auto-populated from DiagnosisRecord.ClinicId during assignment.
    }
}
