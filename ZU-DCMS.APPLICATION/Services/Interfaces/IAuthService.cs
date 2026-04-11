using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // __ Interface for authentication service to handle user registration, login, token refresh, and token revocation __ //
    public interface IAuthService
    {
        Task<Result<AuthDto>> RegisterPatientAsync(RegisterPatientDto dto); // => Method to register a new patient using the provided registration data transfer object
        Task<Result<AuthDto>> LoginAsync(LoginDto dto); // => Method to patients login using the provided login data transfer object
        Task<Result<AuthDto>> StaffLoginAsync(StaffLoginDto dto); // => Method to staff logins using the provided registration data transfer object
    }
}
