using System.Security.Claims;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Token;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Common.Token
{
    /// __ The TokenService class implements the ITokenService interface to handle JWT access token generation, refresh token management, and revocation. __ //
    public class TokenService : ITokenService
    {
        private readonly IJWTService _jwt;
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;

        public TokenService(IJWTService jwt, IUnitOfWork uow, IIdentityService identity)
        {
            _jwt = jwt;
            _uow = uow;
            _identity = identity;
        }

        /// <summary>
        /// Generates a JWT access token and a refresh token for the specified user ID.
        /// Only adds RefreshToken to Repository — caller must SaveChanges.
        /// </summary>
        public async Task<TokenResult> GenerateAsync(string userId)
        {
            // __ Retrieve user information and roles from the identity service __ //
            var user = await _identity.FindByIdAsync(userId);
            var roles = await _identity.GetRolesAsync(userId);

            // __ Create claims for the JWT access token, including user ID, username, email, full name, and roles __ //
            // __ Resolve UserType string (Staff or Patient) __ //
            var userTypeString = user?.UserType;
            if (string.IsNullOrEmpty(userTypeString) || userTypeString == "0")
            {
                userTypeString = roles.Contains("Patient") ? "Patient" : "Staff";
            }
            else if (userTypeString != "Staff" && userTypeString != "Patient")
            {
                // If it's a numeric string or something else, map it
                userTypeString = roles.Contains("Patient") ? "Patient" : "Staff";
            }
            
            // __ Claims identity __ //
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, userId),
                new (ClaimTypes.Name, user?.Username ?? ""),
                new (ClaimTypes.Email, user?.Email     ?? string.Empty),
                new ("fullName",       user?.FullName  ?? string.Empty),
                new ("UserType",       userTypeString)
            };

            // __ Add all roles as claims __ //
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // __ Generate the JWT access token and a new refresh token, then store the refresh token in the database with an expiration date __ //
            var accessToken = _jwt.GenerateAccessToken(claims);
            var refreshToken = _jwt.GenerateRefreshToken();

            var tokenEntity = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Default 7 days
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<RefreshToken>().AddAsync(tokenEntity);
            // Note: Caller must SaveChanges unless this is part of RefreshAsync which handles it.

            // __ Return the generated access token and refresh token __ //
            return TokenResult.Success(new TokenData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }


        // __ Refreshes the JWT access token using the provided refresh token. __ //
        public async Task<TokenResult> RefreshAsync(string refreshToken)
        {
            // __ Retrieve the refresh token from the database and validate its status (active, not revoked) __ //
            var stored = await _uow.Repository<RefreshToken>().GetFirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored == null || !stored.IsActive)
                return TokenResult.Fail(new[] { "الـ Token غير صالح" });

            if (stored.IsRevoked)
                return TokenResult.Fail(new[] { "تم استخدام توكن قديم" });

            var user = await _identity.FindByIdAsync(stored.UserId);
            if (user == null || !user.IsActive)
                return TokenResult.Fail(new[] { "الحساب غير موجود أو موقوف" });

            // __ Begin a database transaction to revoke the old refresh token and generate a new access token and refresh token atomically __ //
            await _uow.BeginTransactionAsync();
            try
            {
                // __ Revoke the old refresh token to prevent reuse __ //
                await RevokeAsync(stored);

                // __ Generate a new access token and refresh token for the user, and update the database with the new refresh token __ //
                var newTokens = await GenerateAsync(stored.UserId);

                if (!newTokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    return newTokens;
                }


                // __ Update the stored refresh token with the new token value to track token replacement __ //
                stored.ReplacedByToken = newTokens.Value.RefreshToken;
                
                _uow.Repository<RefreshToken>().Update(stored);

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return newTokens;
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        // __ Revokes the specified refresh token, marking it as revoked in the database. __ //
        public Task RevokeAsync(RefreshToken stored)
        {
            // __ If the token is null or already inactive, do nothing __ //
            if (stored == null || !stored.IsActive) return Task.CompletedTask;

            // __ Mark the token as revoked and set the revocation timestamp, then update the database __ //
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            _uow.Repository<RefreshToken>().Update(stored);

            // __ Note: Caller must call SaveChanges to persist the revocation __ //
            return Task.CompletedTask;
        }

        public async Task<bool> RevokeByTokenAsync(string token)
        {
            var stored = await _uow.Repository<RefreshToken>().GetFirstOrDefaultAsync(t => t.Token == token);
            if (stored == null || !stored.IsActive) return false;

            await RevokeAsync(stored);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
