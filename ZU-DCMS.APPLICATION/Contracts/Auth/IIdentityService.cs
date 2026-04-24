using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Contracts.Auth
{
    // _________________________ Identity Service Contract _________________________ //
    // __ This interface defines the contract for user identity management, including user creation, role assignment, lookup, validation, and updates. __ //
    public interface IIdentityService
    {
        // ________________________ Get ________________________ // 
        Task<PagedResult<StaffUsersDto>> GetAllUsersAsync(PagedRequest request, string? role = null); 

        // _________________________ Create _________________________ //
        Task<(bool Success, string UserId, string Error)> CreateUserAsync
            (
                string username,
                string? email,
                string phoneNumber,
                string fullName,
                UserType type,
                string password
            );

        // _________________________ Role Management _________________________ //
        Task<bool> AssignRoleAsync(string userId, string role);
        Task<IList<string>> GetRolesAsync(string userId);

        // _________________________ Lookup _________________________ //
        Task<ApplicationUserDto?> FindByUsernameAsync(string username);
        Task<ApplicationUserDto?> FindByPhoneAsync(string phone);
        Task<ApplicationUserDto?> FindByEmailAsync(string email);
        Task<ApplicationUserDto?> FindByIdAsync(string userId);

        // _________________________ Validation _________________________ //
        Task<bool> CheckPasswordAsync(string userId, string password);
        Task<bool> IsActiveAsync(string userId);

        // _________________________ Update _________________________ //
        Task<bool> UpdateUsernameAsync(string userId, string newUsername);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}
