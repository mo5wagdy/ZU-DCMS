using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Admin
{
    // __ DTO for Admin User information __ //
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
