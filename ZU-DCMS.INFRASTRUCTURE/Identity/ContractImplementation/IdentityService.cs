using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.INFRASTRUCTURE.Identity.ContractImplementation
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

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
