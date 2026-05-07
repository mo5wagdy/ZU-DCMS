using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Auth;
using ZU_DCMS.APPLICATION.Common.Token;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

using ZU_DCMS.APPLICATION.Common.Extensions;
namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthDto>>
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

        // _________________________ Patient Login _________________________ //
        public async Task<Result<AuthDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;
            
            // __ Normalize __ //
            dto.PhoneNumber = dto.PhoneNumber.Trim().NormalizeDigits();
            dto.IdentityNumber = dto.IdentityNumber.Trim().NormalizeDigits();

            _logger.LogInfo("Patient login attempt with PhoneNumber: {PhoneNumber}, IdentityNumber: {IdentityNumber}", dto.PhoneNumber, dto.IdentityNumber);

            // __ Find by phone __ //
            var phone = dto.PhoneNumber.Trim();
                
            var user = await _identity.FindByPhoneAsync(phone);

            // __ Generic error for security — don't reveal which field is wrong __ //
            if (user is null)
            {
                _logger.LogWarning("Login failed: User not found for PhoneNumber: {PhoneNumber}", dto.PhoneNumber);
                
                return Result.Failure<AuthDto>("Invalid login credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: Account is inactive for PhoneNumber: {PhoneNumber}", dto.PhoneNumber);
                
                return Result.Failure<AuthDto>("Account suspended, contact administration");
            }

            // __ For patients — password is their identity number __ //
            if (!await _identity.CheckPasswordAsync(user.Id, dto.IdentityNumber))
            {
                _logger.LogWarning("Login failed: Incorrect password for PhoneNumber: {PhoneNumber}", dto.PhoneNumber);
                
                return Result.Failure<AuthDto>("Invalid login credentials");
            }

            // __ Ensure this account is a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);

            if (!roles.Contains(UserRoles.Patient))
            {
                _logger.LogWarning("Login failed: User is not a patient for PhoneNumber: {PhoneNumber}", dto.PhoneNumber);
                
                return Result.Failure<AuthDto>("Invalid login credentials");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Generating tokens for patient: {PhoneNumber}", dto.PhoneNumber);

                // __ Generate tokens __ //
                var tokens = await _token.GenerateAsync(user.Id);

                // __ If token generation failed, return the error __ //
                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    _logger.LogError("Failed to generate tokens for patient");
                    return Result.Failure<AuthDto>(tokens.Error);
                }

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);
                
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Patient login successful: {PhoneNumber}", dto.PhoneNumber);

                return Result.Success<AuthDto>(new AuthDto
                {
                    AccessToken = tokens.Value.AccessToken,
                    RefreshToken = tokens.Value.RefreshToken,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Role = UserRoles.Patient,
                    RedirectUrl = RedirectUrls.GetByRole(UserRoles.Patient)
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("An error occurred during patient login");
                
                return Result.Failure<AuthDto>("An error occurred during login, try again");
            }
        }
    }
}
