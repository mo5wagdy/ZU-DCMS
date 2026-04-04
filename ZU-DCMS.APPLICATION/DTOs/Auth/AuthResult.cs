using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
    // DTO for authentication result
    // This class encapsulates the result of an authentication attempt, including success status, tokens, user role, and any error messages.
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Role { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
