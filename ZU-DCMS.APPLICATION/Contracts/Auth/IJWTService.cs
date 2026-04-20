using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts.Auth
{
    // __ Interface for JWT service to handle token generation and validation __ //
    public interface IJWTService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims); // => Method to generate an access token based on the provided 
        string GenerateRefreshToken(); // => Method to generate a refresh token
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token); // => Method to validate a token and return the claims principal if valid
    }
}

