using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // This interface defines the contract for diagnosis-related operations in the application.
    public interface IDiagnosisService
    {
        // Retrieves a list of patients associated with a specific session for diagnosis purposes.
        Task<List<BookingForDiagnosisDto>> GetSessionPatientsAsync(int sessionId);

        // Retrieves detailed information about a specific patient based on their intern ID.
        Task<DiagnosisRecordDto> DiagnosePatientAsync(string internId, CreateDiagnosisDto dto);

        // Retrieves a list of available students for a specific clinic and term, which can be assigned to patients for diagnosis.
        Task<List<StudentPriorityDto>> GetAvailableStudentsAsync(int clinicId, int termId);

        // Assigns a student to a patient for diagnosis based on the intern ID and the provided assignment details.
        Task<CaseAssignmentDto> AssignStudentAsync(string internId, AssignStudentDto dto);
    }
}
