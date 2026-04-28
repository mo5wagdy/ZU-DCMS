using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.Logout
{
    // __ The LogoutCommand is a record that encapsulates the refresh token required for user logout. It inherits from IRequest<Result<string>>, indicating that it will return either a successful result with a string (success message) or a failure result with errors. __ //
    public record LogoutCommand(string RefreshToken) : IRequest<Result<string>>;
}
