using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.DTOs.Auth
{
    /* 
     * DTO for authentication result
     * This class encapsulates the result of an authentication attempt, 
     * including success status, tokens, user role, and any error messages.
     */
    public class AuthResult : Result<AuthData>
    {
        private AuthResult(AuthData value, bool isSuccess, string error) : base(value, isSuccess, error) { }
        
        public static AuthResult Ok(AuthData data) => new(data, true, string.Empty);

        public static  AuthResult Fail(string error) => new(default!, false, error);
    }

    public class AuthData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
