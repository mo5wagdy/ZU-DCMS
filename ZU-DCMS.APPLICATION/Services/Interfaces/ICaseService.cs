using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // __ This interface defines the contract for case-related operations in the application. __ //
    public interface ICaseService
    {
        Task<Result<List<CaseAssignmentDto>>> GetStudentCasesAsync(int studentId); // => Retrieves a list of case assignments for a specific student, allowing them to view their assigned cases.
        Task<Result<CaseAssignmentDto>> GetByIdAsync(int caseAssignmentId); // => Retrieves detailed information about a specific case assignment based on its ID, allowing students to view case details and progress.
        Task<Result<CaseSessionDto>> AddSessionProgressAsync(int studentId, AddCaseSessionDto dto); // => Adds progress to a specific case session for a student, allowing them to update their case progress and track their work.
        Task<Result> CompleteCaseAsync(int caseAssignmentId, int studentId); // => Marks a specific case assignment as complete for a student,allowing them to indicate that they have finished working on the case and update their progress accordingly.
        Task<Result<StudentProgressDto>> GetStudentProgressAsync(int studentId, int termId); // => Retrieves the progress of a specific student for a given term, allowing them to track their overall progress and performance in their assigned cases.
    }
}
