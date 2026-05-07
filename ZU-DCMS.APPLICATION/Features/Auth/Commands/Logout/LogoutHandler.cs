using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Token;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, Result<string>>
    {
        private readonly ITokenService _tokenService;

        public LogoutHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Result.Failure<string>("Refresh token is required for logout");

            var result = await _tokenService.RevokeByTokenAsync(request.RefreshToken);

            return result  ? Result.Success("Logged out successfully")  : Result.Failure<string>("حدث خطأ أثناء تسجيل الخروج أو Invalid token");
        }
    }
}
