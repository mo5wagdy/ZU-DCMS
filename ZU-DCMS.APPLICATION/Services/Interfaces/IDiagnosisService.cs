using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // __ This interface defines the contract for diagnosis-related operations in the application. __ //
    public interface IDiagnosisService
    {
        // __ This method retrieves a list of patients booked for diagnosis in a specific session, filtered by the intern doctor's ID. __ //
        Task<Result<List<BookingForDiagnosisDto>>> GetSessionPatientsAsync(int sessionId, string internDoctorId);

        // __ This method allows an intern doctor to diagnose a patient by providing the necessary details in the CreateDiagnosisDto. It returns the result of the diagnosis operation, including any relevant information about the diagnosis record. __ //
        Task<Result<DiagnosisRecordDto>> DiagnosePatientAsync(string internDoctorId, CreateDiagnosisDto dto);

        // __ This method retrieves a list of available students for assignment to a case in a specific clinic and term. It returns the result containing a list of StudentPriorityDto, which may include information about the students' priorities for assignment. __ //
        Task<Result<List<StudentPriorityDto>>> GetAvailableStudentsAsync(int clinicId, int termId);

        // __ This method allows an intern doctor to assign a student to a case based on the provided AssignStudentDto. It returns the result of the assignment operation, including details about the assigned case and the student. __ //
        Task<Result<CaseAssignmentDto>> AssignStudentAsync(string internDoctorId, AssignStudentDto dto);
    }
}
