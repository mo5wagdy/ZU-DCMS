using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Auth;
using ZU_DCMS.APPLICATION.Common.Token;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.StaffLogin
{
    public class StaffLoginHandler : IRequestHandler<StaffLoginCommand, Result<AuthDto>>
    {
        private readonly IIdentityService _identity;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _token;
        private readonly IAppLogger<StaffLoginHandler> _logger;

        public StaffLoginHandler(
            IIdentityService identity,
            IUnitOfWork uow,
            ITokenService token,
            IAppLogger<StaffLoginHandler> logger)
        {
            _identity = identity;
            _uow = uow;
            _token = token;
            _logger = logger;
        }

        // _________________________ Staff Login _________________________ //
        public async Task<Result<AuthDto>> Handle(StaffLoginCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;

            _logger.LogInfo("Staff login attempt with Email: {Email}", dto.Email);

            // __ Staff uses email to login __ //
            var user = await _identity.FindByEmailAsync(dto.Email);

            // __ Generic error for security — don't reveal which field is wrong __ //
            if (user is null)
            {
                _logger.LogWarning("Login failed: User not found for Email: {Email}", dto.Email);
                
                return Result.Failure<AuthDto>("Invalid login credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User account is inactive for Email: {Email}", dto.Email);
               
                return Result.Failure<AuthDto>("Account suspended, contact administration");
            }

            if (!await _identity.CheckPasswordAsync(user.Id, dto.Password))
            {
                _logger.LogWarning("Login failed: Incorrect password for Email: {Email}", dto.Email);
                
                return Result.Failure<AuthDto>("Invalid login credentials");
            }

            // __ Ensure this account is Not a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);

            if (roles.Contains(UserRoles.Patient))
            {
                _logger.LogWarning("Login failed: User is a patient, not staff for Email: {Email}", dto.Email);
                
                return Result.Failure<AuthDto>("Invalid login credentials");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Generating tokens for staff: {Email}", dto.Email);

                // __ Generate tokens __ //
                var tokens = await _token.GenerateAsync(user.Id);

                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    
                    _logger.LogError("Failed to generate tokens for staff");
                    
                    return Result.Failure<AuthDto>(tokens.Error);
                }

                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);
                
                await _uow.CommitTransactionAsync();

                // __ Pick the most relevant role for redirection and frontend state __ //
                var role = roles.Contains(UserRoles.Admin) ? UserRoles.Admin 
                         : roles.Contains(UserRoles.TeachingAssistant) ? UserRoles.TeachingAssistant
                         : roles.Contains(UserRoles.Student) ? UserRoles.Student
                         : roles.FirstOrDefault() ?? string.Empty;

                _logger.LogInfo("Staff login successful: {Email}, Role: {Role}", dto.Email, role);

                return Result.Success<AuthDto>(new AuthDto
                {
                    AccessToken = tokens.Value.AccessToken,
                    RefreshToken = tokens.Value.RefreshToken,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Role = role,
                    RedirectUrl = RedirectUrls.GetByRole(role)
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("An error occurred during staff login");
                
                return Result.Failure<AuthDto>("An error occurred during login, try again");
            }
        }
    }
}
