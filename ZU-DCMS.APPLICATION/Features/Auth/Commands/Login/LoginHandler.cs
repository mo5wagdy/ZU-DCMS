using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.Login
{
    public class LoginHandler
    {
        private readonly IIdentityService _identity;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _token;
        private readonly IAppLogger<LoginHandler> _logger;

        public LoginHandler(
            IIdentityService identity,
            IUnitOfWork uow,
            ITokenService token,
            IAppLogger<LoginHandler> logger)
        {
            _identity = identity;
            _uow = uow;
            _token = token;
            _logger = logger;
        }

        public async Task<Result<AuthDto>> Handle(LoginCommand command)
        {
            var dto = command.Dto;

            _logger.LogInfo("Patient login attempt with Username: {Username}, IdentityNumber: {IdentityNumber}", dto.Username, dto.IdentityNumber);

            var user = await _identity.FindByUsernameAsync(dto.Username);

            if (user is null)
            {
                _logger.LogWarning("Login failed: User not found for Username: {Username}", dto.Username);
                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: Account is inactive for Username: {Username}", dto.Username);
                return Result.Failure<AuthDto>("الحساب موقوف، تواصل مع الإدارة");
            }

            if (!await _identity.CheckPasswordAsync(user.Id, dto.IdentityNumber))
            {
                _logger.LogWarning("Login failed: Incorrect password for Username: {Username}", dto.Username);
                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            var roles = await _identity.GetRolesAsync(user.Id);

            if (!roles.Contains(UserRoles.Patient))
            {
                _logger.LogWarning("Login failed: User is not a patient for Username: {Username}", dto.Username);
                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Generating tokens for patient: {Username}", dto.Username);

                var tokens = await _token.GenerateAsync(user.Id);

                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    _logger.LogError("Failed to generate tokens for patient");
                    return Result.Failure<AuthDto>(tokens.Error);
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Patient login successful: {Username}", dto.Username);

                return Result.Success<AuthDto>(new AuthDto
                {
                    AccessToken = tokens.Value.AccessToken,
                    RefreshToken = tokens.Value.RefreshToken,
                    Role = UserRoles.Patient,
                    RedirectUrl = RedirectUrls.GetByRole(UserRoles.Patient)
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("An error occurred during patient login");
                return Result.Failure<AuthDto>("حدث خطأ أثناء تسجيل الدخول، حاول مرة أخرى");
            }
        }
    }
}
