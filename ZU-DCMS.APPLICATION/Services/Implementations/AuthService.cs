using AutoMapper;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityService _identity;
        private readonly IUnitOfWork _uow;
        private readonly IRawSqlExecutor _sql;
        private readonly IUserCodeGenerator _codeGen;
        private readonly IJWTService _jwt;
        //private readonly INotificationService _notification;
        private readonly IMapper _mapper;

        public AuthService
        (
            IIdentityService identity,
            IUnitOfWork uow,
            IRawSqlExecutor sql,
            IUserCodeGenerator codegen,
            IJWTService jwt,
           // INotificationService notification,
            IMapper mapper
        )
        {
            _identity = identity;
            _uow = uow;
            _sql = sql;
            _codeGen = codegen;
            _jwt = jwt;
            //_notification = notification;
            _mapper = mapper;
        }

        // _________________________ Patient Registration _________________________ //
        public async Task<AuthResult> RegisterPatientAsync(RegisterPatientDto dto)
        {
            // __ Check username uniqueness __ //
            if (await _identity.UsernameExistsAsync(dto.Username))
                return AuthResult.Fail("اسم المستخدم موجود بالفعل");

            // __ Check identity number uniqueness __ //
            if (await _uow.Repository<Patient>().ExistsAsync(p => p.IdentityNumber == dto.IdentityNumber))
                return Failure("رقم الهوية مسجل بالفعل");

            // __ Begin transaction — Identity + Patient must both succeed __ //
            await _uow.BeginTransactionAsync();
            try
            {
                // __ Create Identity user — password is the identity number __ //
                var (success, userId, error) = await _identity.CreateUserAsync
                (
                    dto.Username,
                    dto.Email,
                    dto.FullName,
                    dto.IdentityNumber
                );

                if (!success) 
                {
                    await _uow.RollbackTransactionAsync();
                    return Failure(error); 
                }

                // __ Assign Patient role __ //
                await _identity.AssignRoleAsync(userId, UserRoles.Patient);

                // Map DTO → Entity then fill system-generated fields
                var patient = _mapper.Map<Patient>(dto);
                patient.ApplicationUserId = userId;
                patient.PatientCode = await _codeGen.GenerateAsync("PAT", "PatientCodeSeq");
                patient.CreatedAt = DateTime.UtcNow;

                await _uow.Repository<Patient>().AddAsync(patient);

                // __ Prepare tokens (AddAsync only — no SaveChanges yet) __ //
                var (accessToken, refreshToken) = await PrepareTokensAsync(userId);

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return AuthResult.Ok(new AuthData
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Role = UserRoles.Patient,
                    RedirectUrl = RedirectUrls.GetByRole(UserRoles.Patient)
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                return Failure("حدث خطأ أثناء التسجيل، حاول مرة أخرى");
            }
        }

        // _________________________ Patient Login _________________________ //
        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            // __ Find by username __ //
            var user = await _identity.FindByUsernameAsync(dto.Username);

            // __ Generic error for security — don't reveal which field is wrong __ //
            if (user == null)
                return Failure("بيانات الدخول غير صحيحة");

            if (!user.IsActive)
                return Failure("الحساب موقوف، تواصل مع الإدارة");

            // __ For patients — password is their identity number __ //
            if (!await _identity.CheckPasswordAsync(user.Id, dto.IdentityNumber))
                return Failure("بيانات الدخول غير صحيحة");

            // __ Ensure this account is a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);
            if (!roles.Contains(UserRoles.Patient))
                return Failure("بيانات الدخول غير صحيحة");

            var (accessToken, refreshToken) = await PrepareTokensAsync(user.Id);
            await _uow.SaveChangesAsync();

            return AuthResult.Ok(new AuthData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = UserRoles.Patient,
                RedirectUrl = RedirectUrls.GetByRole(UserRoles.Patient)
            });
        }

        // _________________________ Staff Login _________________________ //
        public async Task<AuthResult> StaffLoginAsync(StaffLoginDto dto)
        {
            // __ Staff uses email to login __ //
            var user = await _identity.FindByEmailAsync(dto.Email);

            if (user == null)
                return Failure("بيانات الدخول غير صحيحة");

            if (!user.IsActive)
                return Failure("الحساب موقوف، تواصل مع الإدارة");

            if (!await _identity.CheckPasswordAsync(user.Id, dto.Password))
                return Failure("بيانات الدخول غير صحيحة");

            // __ Staff must NOT be a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);
            if (roles.Contains(UserRoles.Patient))
                return Failure("بيانات الدخول غير صحيحة");

            var (accessToken, refreshToken) = await PrepareTokensAsync(user.Id);
            await _uow.SaveChangesAsync();

            var role = roles.FirstOrDefault() ?? string.Empty;

            return AuthResult.Ok(new AuthData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = role,
                RedirectUrl = RedirectUrls.GetByRole(role)
            });
        }

        // _____________ Refresh Token _____________ //
        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            // Find stored token
            var stored = await _uow.Repository<RefreshToken>().GetFirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored == null || !stored.IsActive)
                return Failure("الـ Token غير صالح");

            // Verify user still valid
            var user = await _identity.FindByIdAsync(stored.UserId);
            if (user == null || !user.IsActive)
                return Failure("الحساب غير موجود أو موقوف");

            // Revoke old token — keep audit trail
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            _uow.Repository<RefreshToken>().Update(stored);

            // Generate new tokens
            var (newAccessToken, newRefreshToken) = await PrepareTokensAsync(stored.UserId);

            // Link old → new for audit
            stored.ReplacedByToken = newRefreshToken;

            await _uow.SaveChangesAsync();

            return AuthResult.Ok(new AuthData
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        // _____________ Revoke Token _____________ //
        public async Task RevokeTokenAsync(string refreshToken)
        {
            var stored = await _uow.Repository<RefreshToken>()
                .GetFirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored == null || !stored.IsActive) return;

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            _uow.Repository<RefreshToken>().Update(stored);
            await _uow.SaveChangesAsync();
        }

        // _____________ Private Helpers _____________ //

        /// <summary>
        /// Builds JWT claims from user data and roles.
        /// </summary>
        private async Task<IEnumerable<Claim>> BuildClaimsAsync(string userId)
        {
            var user = await _identity.FindByIdAsync(userId);
            var roles = await _identity.GetRolesAsync(userId);

            var claims = new List<Claim>
            {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name,  user?.Username  ?? string.Empty),
            new(ClaimTypes.Email, user?.Email     ?? string.Empty),
            new("fullName",       user?.FullName  ?? string.Empty),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            return claims;
        }

        /// <summary>
        /// Generates Access + Refresh tokens.
        /// Only adds RefreshToken to Repository — caller must SaveChanges.
        /// </summary>
        private async Task<(string AccessToken, string RefreshToken)>PrepareTokensAsync(string userId)
        {
            var claims = await BuildClaimsAsync(userId);
            var accessToken = _jwt.GenerateAccessToken(claims);
            var refreshToken = _jwt.GenerateRefreshToken();

            // __ Add to repo — SaveChanges is caller's responsibility __ //
            await _uow.Repository<RefreshToken>().AddAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Creates a failed AuthResult.
        /// </summary>
        private static AuthResult Failure(string message) => AuthResult.Fail(message);
    }
}
