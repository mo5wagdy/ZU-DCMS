using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.Services.Interfaces;
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
                
                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User account is inactive for Email: {Email}", dto.Email);
               
                return Result.Failure<AuthDto>("الحساب موقوف، تواصل مع الإدارة");
            }

            if (!await _identity.CheckPasswordAsync(user.Id, dto.Password))
            {
                _logger.LogWarning("Login failed: Incorrect password for Email: {Email}", dto.Email);
                
                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            // __ Ensure this account is Not a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);

            if (roles.Contains(UserRoles.Patient))
            {
                _logger.LogWarning("Login failed: User is a patient, not staff for Email: {Email}", dto.Email);
                
                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
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

                await _uow.SaveChangesAsync();
                
                await _uow.CommitTransactionAsync();

                var role = roles.FirstOrDefault() ?? string.Empty;

                _logger.LogInfo("Staff login successful: {Email}, Role: {Role}", dto.Email, role);

                return Result.Success<AuthDto>(new AuthDto
                {
                    AccessToken = tokens.Value.AccessToken,
                    RefreshToken = tokens.Value.RefreshToken,
                    Role = role,
                    RedirectUrl = RedirectUrls.GetByRole(role)
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("An error occurred during staff login");
                
                return Result.Failure<AuthDto>("حدث خطأ أثناء تسجيل الدخول، حاول مرة أخرى");
            }
        }
    }
}
