using AutoMapper;
using Microsoft.Extensions.Options;
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
        private readonly JwtSettings _settings;
        private readonly IMapper _mapper;
        private readonly IAppLogger<AuthService> _logger;

        public AuthService
        (
            IIdentityService identity,
            IUnitOfWork uow,
            IUserCodeGenerator codeGen,
            ITokenService token,
            IOptions<JwtSettings> settings,
            IMapper mapper,
            IAppLogger<AuthService> logger

        )
        {
            _identity = identity;
            _uow = uow;
            _codeGen = codeGen;
            _token = token;
            _settings = settings.Value;
            _mapper = mapper;
            _logger = logger;
        }

        // _________________________ Patient Registration _________________________ //
        public async Task<Result<AuthDto>> RegisterPatientAsync(RegisterPatientDto dto)
        {
            // patients login with phone number and Nid
            _logger.LogInfo("Registering patient with Username: {Username}, IdentityNumber: {IdentityNumber}", dto.Username, dto.IdentityNumber);

            // __ Check username uniqueness __ //
            if (await _identity.UsernameExistsAsync(dto.Username))
            {
                _logger.LogWarning("Username already exists: {Username}", dto.Username);

                return Result.Failure<AuthDto>("اسم المستخدم موجود بالفعل");
            }

            // __ Check identity number uniqueness __ //
            if (await _uow.Repository<Patient>().ExistsAsync(p => p.IdentityNumber == dto.IdentityNumber))
            {
                _logger.LogWarning("Identity number already exists: {IdentityNumber}", dto.IdentityNumber);

                return Result.Failure<AuthDto>("رقم الهوية مسجل بالفعل");
            }

            // __ Begin transaction — Identity + Patient must both succeed __ //
            await _uow.BeginTransactionAsync();
            try
            {

                _logger.LogInfo("Creating Identity user for patient: {Username}", dto.Username);

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

                    _logger.LogError("Failed to create Identity user for patient");

                    return Result.Failure<AuthDto>(error); 
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

                    _logger.LogError("Failed to generate tokens for patient");

                    return Result.Failure<AuthDto>(tokens.Error);
                }
                
                await _uow.Repository<RefreshToken>().AddAsync(new RefreshToken
                {
                    Token = tokens.Value.RefreshToken,
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays)
                });

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Patient registration successful: {Username}", dto.Username);

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

                _logger.LogError("An error occurred during patient registration");

                return Result.Failure<AuthDto>("حدث خطأ أثناء التسجيل، حاول مرة أخرى");
            }
        }

        // _________________________ Patient Login _________________________ //
        public async Task<Result<AuthDto>> LoginAsync(LoginPatientDto dto)
        {
            _logger.LogInfo("Patient login attempt with Username: {Username}, IdentityNumber: {IdentityNumber}", dto.Username, dto.IdentityNumber);

            // __ Find by username __ //

            // update => Phone number instead of username
            var user = await _identity.FindByUsernameAsync(dto.Username);

            // __ Generic error for security — don't reveal which field is wrong __ //
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

            // __ For patients — password is their identity number __ //
            if (!await _identity.CheckPasswordAsync(user.Id, dto.IdentityNumber))
            { 
                _logger.LogWarning("Login failed: Incorrect password for Username: {Username}", dto.Username);

                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            // __ Ensure this account is a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);

            if (!roles.Contains(UserRoles.Patient))
            {
                _logger.LogWarning("Login failed: User is not a patient for Username: {Username}", dto.Username);

                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            // __ Generate tokens __ //
            await _uow.BeginTransactionAsync(); // => Transaction needed if token generation modifies the database (e.g. refresh tokens)

            try
            {
                _logger.LogInfo("Generating tokens for patient: {Username}", dto.Username);

                // __ Prepare tokens (no SaveChanges yet) __ //
                var tokens = await _token.GenerateAsync(user.Id);

                // __ If token generation failed, return the error __ //
                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();

                    _logger.LogError("Failed to generate tokens for patient");

                    return Result.Failure<AuthDto>(tokens.Error);
                }

                // __ Single SaveChanges for everything __ //
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

        // _________________________ Staff Login _________________________ //
        public async Task<Result<AuthDto>> StaffLoginAsync(StaffLoginDto dto)
        {
            _logger.LogInfo("Staff login attempt with Email: {Email}", dto.Email);

            // __ Staff uses email to login __ //
            var user = await _identity.FindByEmailAsync(dto.Email);

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

            // __ Staff must NOT be a Patient __ //
            var roles = await _identity.GetRolesAsync(user.Id);

            if (roles.Contains(UserRoles.Patient))
            {
                _logger.LogWarning("Login failed: User is a patient, not staff for Email: {Email}", dto.Email);

                return Result.Failure<AuthDto>("بيانات الدخول غير صحيحة");
            }

            // __ Generate tokens __ //
            await _uow.BeginTransactionAsync(); // => Transaction needed if token generation modifies the database (e.g. refresh tokens)

            try
            {
                _logger.LogInfo("Generating tokens for staff: {Email}", dto.Email);

                // __ Prepare tokens (no SaveChanges yet) __ //
                var tokens = await _token.GenerateAsync(user.Id);

                // __ If token generation failed, return the error __ //
                if (!tokens.IsSuccess)
                {
                    await _uow.RollbackTransactionAsync();

                    _logger.LogError("Failed to generate tokens for staff");

                    return Result.Failure<AuthDto>(tokens.Error);
                }

                // __ Single SaveChanges for everything __ //
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                // __ Get the first role for redirect URL (assuming staff have only one role) __ //
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
