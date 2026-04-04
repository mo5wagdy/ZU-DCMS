using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ZU_DCMS.APPLICATION.JWT
{
    public interface IJWTService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}

