using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZU_DCMS.APPLICATION.Common.Auth;
using ZU_DCMS.APPLICATION.Contracts.Auth;

namespace ZU_DCMS.INFRASTRUCTURE.Identity.ContractImplementation
{
    public class JwtService : IJWTService
    {
        private readonly JwtSettings _settings;
        private readonly SigningCredentials _signingCredentials;

        public JwtService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;

            // __ Build signing credentials once — reuse across all token generations __ //
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        // _________________________ Access Token _________________________ //

        /// <summary>
        /// Generates a signed JWT Access Token containing the provided claims.
        /// Expiry is controlled by JwtSettings.AccessTokenExpiryMinutes.
        /// </summary>
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken
            (
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
                signingCredentials: _signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // _________________________ Refresh Token _________________________ //

        /// <summary>
        /// Generates a cryptographically secure random Refresh Token.
        /// This is NOT a JWT — it's a random string stored in DB.
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        // _________________________ Validate Expired Token _________________________ //

        /// <summary>
        /// Extracts and validates claims from an EXPIRED Access Token.
        /// Used during Refresh Token rotation to verify token identity.
        /// Returns null if the token is invalid (bad signature, wrong format).
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                // __ Validate signature even if expired __ //
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),

                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,

                ValidateAudience = true,
                ValidAudience = _settings.Audience,

                // __ Allow expired tokens — we only care about the signature __ //
                ValidateLifetime = false
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token,validationParameters,out var validatedToken);

                // __ Ensure it was signed with HmacSha256 — reject other algorithms __ //
                if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                // __ Invalid token format or signature __ //
                return null;
            }
        }
    }
}