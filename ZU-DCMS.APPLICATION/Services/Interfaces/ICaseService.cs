using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // This interface defines the contract for case-related operations in the application.
    public interface ICaseService
    {
        // Retrieves a list of case assignments for a specific student, allowing them to view their assigned cases.
        Task<List<CaseAssignmentDto>> GetStudentCasesAsync(int studentId);

        // Retrieves detailed information about a specific case assignment based on its ID, allowing students to view case details and progress.
        Task<CaseAssignmentDto?> GetByIdAsync(int id);

        // Adds progress to a specific case session for a student, allowing them to update their case progress and track their work.
        Task<CaseSessionDto> AddSessionProgressAsync(int studentId, AddCaseSessionDto dto);

        // Marks a specific case assignment as complete for a student,
        // allowing them to indicate that they have finished working on the case and update their progress accordingly.
        Task CompleteCaseAsync(int caseAssignmentId, int studentId);

        // Retrieves the progress of a specific student for a given term, allowing them to track their overall progress and performance in their assigned cases.
        Task<StudentProgressDto> GetStudentProgressAsync(int studentId, int termId);
    }

}
