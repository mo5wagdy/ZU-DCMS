using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Token;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthDto>>
    {
        private readonly ITokenService _tokenService;
        private readonly IIdentityService _identity;

        public RefreshTokenHandler(ITokenService tokenService, IIdentityService identity)
        {
            _tokenService = tokenService;
            _identity = identity;
        }

        public async Task<Result<AuthDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = await _tokenService.RefreshAsync(request.RefreshToken);

            if (!result.IsSuccess)
                return Result.Failure<AuthDto>(result.Errors);

            var user = await _identity.FindByIdAsync(result.Value.UserId);
            var roles = await _identity.GetRolesAsync(result.Value.UserId);

            return Result.Success(new AuthDto
            {
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken,
                UserId = user?.Id ?? string.Empty,
                FullName = user?.FullName ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "",
                RedirectUrl = "/dashboard" // Standard redirect
            });
        }
    }
}
