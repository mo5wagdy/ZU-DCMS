using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // This interface defines the contract for student-related operations in the application.
    public interface IStudentService
    {
        // Retrieves detailed information about a specific student based on their ID, allowing them to view their profile and related information.
        Task<StudentDto?> GetByIdAsync(int id);

        // Retrieves detailed information about a specific student based on their user ID, allowing them to view their profile and related information.
        Task<StudentDto?> GetByUserIdAsync(string userId);

        // Retrieves a paginated list of students based on the provided request parameters, allowing for efficient retrieval and display of student data in a paginated format.
        Task<PagedResult<StudentDto>> GetAllAsync(PagedRequest request);

        // Retrieves a list of student requirements for a specific student and term, allowing them to view the requirements they need to fulfill for that term.
        Task<List<StudentRequirementDto>> GetRequirementsAsync(int studentId, int termId);
    }
}
