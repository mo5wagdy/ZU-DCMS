
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
        Task ToggleUserActiveAsync(string userId);

        // System Config 
        // Retrieves a list of all system configurations, allowing administrators to view and manage the application's configuration settings.
        Task<List<SystemConfigDto>> GetAllConfigsAsync();
        Task UpdateConfigAsync(string key, string value, string adminId);

        // Term
        // Retrieves a list of all terms in the system, allowing administrators to view and manage academic terms.
        Task<List<TermDto>> GetAllTermsAsync();
        Task<TermDto?> GetTermByIdAsync(int termId);
        Task<TermDto> CreateTermAsync(CreateTermDto dto, string adminId);
        Task<TermDto> UpdateTermAsync(int termId, UpdateTermDto dto, string adminId);
        Task SetActiveTermAsync(int termId, string adminId);

        // Student Requirements
        // Retrieves a list of student requirements for a specific student and term,
        // allowing administrators to view and manage the requirements that students need to fulfill for that term.
        Task SetStudentRequirementsAsync(int studentId, int termId, List<SetRequirementDto> requirements, string adminId);
        Task<List<StudentRequirementDto>> GetStudentRequirementsAsync(int studentId, int termId);
        Task TransferRequirementsAsync(int studentId, int fromTermId, int toTermId, string adminId);
    }
}
