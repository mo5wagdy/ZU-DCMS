
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // This interface defines the contract for administrative operations in the application,
    // allowing administrators to manage users,
    // system configurations, terms, and student requirements.
    public interface IAdminService
    {
        // Users 
        // Retrieves a list of all users in the system, allowing administrators to view and manage user accounts.
        Task<Result<PagedResult<StaffUsersDto>>> GetAllUsersAsync(PagedRequest request, string role);
        Task<Result<StaffUsersDto>> GetUserByIdAsync(string userId);
        Task<Result<StaffUsersDto>> CreateUserAsync(CreateUserDto dto);
        Task<Result> ToggleUserActiveAsync(string userId);

        // System Config 
        // Retrieves a list of all system configurations, allowing administrators to view and manage the application's configuration settings.
        Task<Result<List<SystemConfigDto>>> GetAllConfigsAsync();
        Task<Result> UpdateConfigAsync(string key, string value, string adminId);

        // Term
        // Retrieves a list of all terms in the system, allowing administrators to view and manage academic terms.
        Task<Result<List<TermDto>>> GetAllTermsAsync();
        Task<Result<TermDto>> GetTermByIdAsync(int termId);
        Task<Result<TermDto>> CreateTermAsync(CreateTermDto dto, string adminId);
        Task<Result<TermDto>> UpdateTermAsync(int termId, UpdateTermDto dto, string adminId);
        Task<Result> SetActiveTermAsync(int termId, string adminId);

        // Student Requirements
        // Retrieves a list of student requirements for a specific student and term,
        // allowing administrators to view and manage the requirements that students need to fulfill for that term.
        Task<Result> SetStudentRequirementsAsync(int studentId, int termId, List<SetRequirementDto> requirements, string adminId);
        Task<Result<List<StudentRequirementDto>>> GetStudentRequirementsAsync(int studentId, int termId);
        Task<Result> TransferRequirementsAsync(int studentId, int fromTermId, int toTermId, string adminId);
    }
}
