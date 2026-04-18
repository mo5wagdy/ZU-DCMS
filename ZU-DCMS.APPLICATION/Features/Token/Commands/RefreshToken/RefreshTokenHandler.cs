using System.Security.Claims;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Token;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Token.Commands.RefreshToken
{
    public class RefreshTokenHandler
    {
        private readonly IJWTService _jwt;
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;

        public RefreshTokenHandler(IJWTService jwt, IUnitOfWork uow, IIdentityService identity)
        {
            _jwt = jwt;
            _uow = uow;
            _identity = identity;
        }

        public async Task<TokenResult> Handle(RefreshTokenCommand command)
        {
            var refreshToken = command.RefreshToken;

            var stored = await _uow.Repository<Domain.Entities.RefreshToken>().GetFirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored == null || !stored.IsActive)
                return TokenResult.Fail("الـ Token غير صالح");

            if (stored.IsRevoked)
                return TokenResult.Fail("تم استخدام توكن قديم");

            var user = await _identity.FindByIdAsync(stored.UserId);
            if (user == null || !user.IsActive)
                return TokenResult.Fail("الحساب غير موجود أو موقوف");

            await _uow.BeginTransactionAsync();
            try
            {
                await RevokeAsync(stored);

                var newTokens = await GenerateAsync(stored.UserId);

                if (!newTokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    return newTokens;
                }

                stored.ReplacedByToken = newTokens.Value.RefreshToken;
                
                _uow.Repository<Domain.Entities.RefreshToken>().Update(stored);

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

        private async Task<TokenResult> GenerateAsync(string userId)
        {
            var user = await _identity.FindByIdAsync(userId);
            var roles = await _identity.GetRolesAsync(userId);

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, userId),
                new (ClaimTypes.Name, user?.Username ?? ""),
                new (ClaimTypes.Email, user?.Email     ?? string.Empty),
                new ("fullName",       user?.FullName  ?? string.Empty)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var accessToken = _jwt.GenerateAccessToken(claims);
            var refreshToken = _jwt.GenerateRefreshToken();

            return TokenResult.Success(new TokenData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        private Task RevokeAsync(Domain.Entities.RefreshToken stored)
        {
            if (stored == null || !stored.IsActive) return Task.CompletedTask;

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            _uow.Repository<Domain.Entities.RefreshToken>().Update(stored);

            return Task.CompletedTask;
        }
    }
}
