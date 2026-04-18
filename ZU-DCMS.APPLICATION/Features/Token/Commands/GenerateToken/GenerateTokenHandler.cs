using System.Security.Claims;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Token;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Token.Commands.GenerateToken
{
    public class GenerateTokenHandler
    {
        private readonly IJWTService _jwt;
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;

        public GenerateTokenHandler(IJWTService jwt, IUnitOfWork uow, IIdentityService identity)
        {
            _jwt = jwt;
            _uow = uow;
            _identity = identity;
        }

        public async Task<TokenResult> Handle(GenerateTokenCommand command)
        {
            var userId = command.UserId;

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
    }
}
