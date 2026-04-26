using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.UserRoles;
using ZU_DCMS.INFRASTRUCTURE.Persistence;

namespace ZU_DCMS.INFRASTRUCTURE.Identity.ContractImplementation
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public IdentityService(UserManager<ApplicationUser> userManager, AppDbContext context, IMapper mapper) {_userManager = userManager; _context = context; _mapper = mapper;} 

        // ________________________ Get ________________________ // 
        public async Task<PagedResult<StaffUsersDto>> GetAllUsersAsync(PagedRequest request, string? role = null)
        {
            var users =  _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                //var usersInRole = await _userManager.GetUsersInRoleAsync(role);

                //users = usersInRole.AsQueryable();

                users = users.Where
                    (
                        u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId &&
                        r.Name == role && 
                        r.Name != "Patient"))
                    );
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

            var items = await users
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize).ToListAsync();

            var usersDtos = new List<StaffUsersDto>(); 
            
            foreach (var i in items)
            {
                var userRoles = await _userManager.GetRolesAsync(i);

                usersDtos.Add(new StaffUsersDto
                {
                    Id = i.Id,
                    FullName = i.FullName,
                    Username = i.UserName ?? string.Empty,
                    Email = i.Email,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt,
                    Role = userRoles.FirstOrDefault() ?? string.Empty
                });
            }


            var result = PagedResult<StaffUsersDto>.Create(usersDtos, totalCount, request);

            return result;
        }

        // _________________________ Create _________________________ //
        public async Task<(bool Success, string UserId, string Error)> CreateUserAsync
        (
            string username,
            string? email,
            string? phoneNumber,
            string fullName,
            UserType type,
            string password
        )
        {
            var user = new ApplicationUser
            {
                UserName = username.Trim().ToLower(),
                Email = email?.Trim().ToLower(),
                PhoneNumber = phoneNumber?.Trim(),
                PhoneNumberConfirmed = true,
                FullName = fullName.Trim(),
                UserType = type,
                EmailConfirmed = email != null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim("UserType", type.ToString()));
            }

            return result.Succeeded ? (true, user.Id, string.Empty) : (false, string.Empty, result.Errors.First().Description);
        }

        // _________________________ Role Management _________________________ //
        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        // _________________________ Lookup _________________________ //
        public async Task<ApplicationUserDto?> FindByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username.Trim().ToLower());
            return user is null ? null : MapToDto(user);
        }

        public async Task<ApplicationUserDto?> FindByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var userExsist = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync
                (
                    u => u.Id != null &&
                    u.PhoneNumber == phone.Trim() &&
                    u.IsActive &&
                    u.PhoneNumber != null &&
                    u.PhoneNumber != ""
                );

            return userExsist is null ? null : MapToDto(userExsist); 
        }

        public async Task<ApplicationUserDto?> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            return user is null ? null : MapToDto(user);
        }

        public async Task<ApplicationUserDto?> FindByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user is null ? null : MapToDto(user);
        }

        // _________________________ Validation _________________________ //

        public async Task<bool> CheckPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

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
            if (user is null) return false;

            user.UserName = newUsername.Trim().ToLower();
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UsernameExistsAsync(string username) => await _userManager.FindByNameAsync(username.Trim().ToLower()) != null;

        public async Task<bool> EmailExistsAsync(string email) => await _userManager.FindByEmailAsync(email.Trim().ToLower()) != null;

        // _________________________ Private Helpers _________________________ //
        private static ApplicationUserDto MapToDto(ApplicationUser user, string? role = null)
        {
            if (user is null)
                return null!;

            return new()
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = role ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}