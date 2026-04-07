using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
    // __ DTO for user login __ //
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
    }
}
