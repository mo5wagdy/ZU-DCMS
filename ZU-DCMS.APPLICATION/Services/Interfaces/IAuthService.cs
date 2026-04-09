using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // __ Interface for authentication service to handle user registration, login, token refresh, and token revocation __ //
    public interface IAuthService
    {
        Task<AuthResult> RegisterPatientAsync(RegisterPatientDto dto); // => Method to register a new patient using the provided registration data transfer object
        Task<AuthResult> LoginAsync(LoginDto dto); // => Method to register a new doctor using the provided registration data transfer object
        Task<AuthResult> StaffLoginAsync(StaffLoginDto dto); // => Method to staff logins using the provided registration data transfer object
        Task<AuthResult> RefreshTokenAsync(string refreshToken); // => Method to refresh an access token using the provided refresh 
        Task RevokeTokenAsync(string refreshToken); // => Method to revoke a refresh token, effectively logging the user out
    }
}
