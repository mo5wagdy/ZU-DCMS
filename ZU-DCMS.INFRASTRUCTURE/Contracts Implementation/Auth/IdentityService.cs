using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts.Auth;

namespace ZU_DCMS.INFRASTRUCTURE.Identity.ContractImplementation
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public IdentityService(UserManager<ApplicationUser> userManager, IMapper mapper) {_userManager = userManager; _mapper = mapper;} 

        // ________________________ Get ________________________ // 
        public async Task<PagedResult<StaffUsersDto>> GetAllUsersAsync(PagedRequest request, string? role = null)
        {
            var users =  _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);

                users = usersInRole.AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = (request.SearchTerm ?? "").Trim().ToLower();

                users = users.Where
                    (
                        u => u.UserName != null && u.UserName.ToLower().Contains(searchTerm) ||
                        u.FullName != null && u.FullName.ToLower().Contains(searchTerm) ||
                        u.Email != null && u.Email.ToLower().Contains(searchTerm)
                    );
            }

            var sortBy = request.SortBy?.Trim().ToLower() ?? "fullname";

            if (request.SortDescending)
            {
                users = sortBy switch
                {
                    "fullname" => users.OrderByDescending(u => u.FullName),
                    "email" => users.OrderByDescending(u => u.Email),
                    _ => users.OrderByDescending(u => u.Id)
                };
            }
            else
            {
                users = sortBy switch
                {
                    "fullname" => users.OrderBy(u => u.FullName),
                    "email" => users.OrderBy(u => u.Email),
                    _ => users.OrderBy(u => u.Id)
                };
            }

            var totalCount = await users.CountAsync();

            var items = users
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize).ToListAsync();

            var usersDtos = _mapper.Map<List<StaffUsersDto>>(items);

            var result = PagedResult<StaffUsersDto>.Create(usersDtos, totalCount, request);

            return result;
        }

        // _________________________ Create _________________________ //
        public async Task<(bool Success, string UserId, string Error)> CreateUserAsync
        (
            string username,
            string? email,
            string fullName,
            string password
        )
        {
            var user = new ApplicationUser
            {
                UserName = username.Trim().ToLower(),
                Email = email?.Trim().ToLower(),
                FullName = fullName.Trim(),
                EmailConfirmed = email != null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);

            return result.Succeeded ? (true, user.Id, string.Empty) : (false, string.Empty, result.Errors.First().Description);
        }

        // _________________________ Role Management _________________________ //
        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        // _________________________ Lookup _________________________ //
        public async Task<ApplicationUserDto?> FindByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username.Trim().ToLower());
            return user == null ? null : MapToDto(user);
        }

        public async Task<ApplicationUserDto?> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            return user == null ? null : MapToDto(user);
        }

        public async Task<ApplicationUserDto?> FindByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? null : MapToDto(user);
        }

        // _________________________ Validation _________________________ //

        public async Task<bool> CheckPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> IsActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.IsActive ?? false;
        }

        // _________________________ Update _________________________ //
        public async Task<bool> UpdateUsernameAsync(string userId, string newUsername)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.UserName = newUsername.Trim().ToLower();
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UsernameExistsAsync(string username) => await _userManager.FindByNameAsync(username.Trim().ToLower()) != null;

        public async Task<bool> EmailExistsAsync(string email) => await _userManager.FindByEmailAsync(email.Trim().ToLower()) != null;

        // _________________________ Private Helpers _________________________ //
        private static ApplicationUserDto MapToDto(ApplicationUser user) => new()
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}