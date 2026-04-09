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
        private readonly IUserCodeGenerator _codeGen;
        private readonly ITokenService _token;
        private readonly IMapper _mapper;

        public AuthService
        (
            IIdentityService identity,
            IUnitOfWork uow,
            IUserCodeGenerator codeGen,
            ITokenService token,
            IMapper mapper
        )
        {
            _identity = identity;
            _uow = uow;
            _codeGen = codeGen;
            _token = token;
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
                var tokens = await _token.GenerateAsync(userId);

                // __ If token generation failed, return the error __ //
                if (!tokens.IsSuccess) 
                {
                    await _uow.RollbackTransactionAsync();
                    return Failure(tokens.Error);
                }
                    

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return AuthResult.Ok(new AuthData
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

            // __ Generate tokens __ //
            await _uow.BeginTransactionAsync(); // => Transaction needed if token generation modifies the database (e.g. refresh tokens)

            try
            {
                // __ Prepare tokens (no SaveChanges yet) __ //
                var tokens = await _token.GenerateAsync(user.Id);

                // __ If token generation failed, return the error __ //
                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    return Failure(tokens.Error);
                }

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return AuthResult.Ok(new AuthData
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
                return Failure("حدث خطأ أثناء تسجيل الدخول، حاول مرة أخرى");
            }
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

            // __ Generate tokens __ //
            await _uow.BeginTransactionAsync(); // => Transaction needed if token generation modifies the database (e.g. refresh tokens)

            try
            {
                // __ Prepare tokens (no SaveChanges yet) __ //
                var tokens = await _token.GenerateAsync(user.Id);

                // __ If token generation failed, return the error __ //
                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();
                    return Failure(tokens.Error);
                }

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                var role = roles.FirstOrDefault() ?? string.Empty;

                return AuthResult.Ok(new AuthData
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
                return Failure("حدث خطأ أثناء تسجيل الدخول، حاول مرة أخرى");
            }
        }

        // _____________ Private Helpers _____________ //
        /// <summary>
        /// Creates a failed AuthResult.
        /// </summary>
        private static AuthResult Failure(string message) => AuthResult.Fail(message);
    }
}
