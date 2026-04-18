using AutoMapper;
using Microsoft.Extensions.Options;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.RegisterPatient
{
    public class RegisterPatientHandler
    {
        private readonly IIdentityService _identity;
        private readonly IUnitOfWork _uow;
        private readonly IUserCodeGenerator _codeGen;
        private readonly ITokenService _token;
        private readonly JwtSettings _settings;
        private readonly IMapper _mapper;
        private readonly IAppLogger<RegisterPatientHandler> _logger;

        public RegisterPatientHandler(
            IIdentityService identity,
            IUnitOfWork uow,
            IUserCodeGenerator codeGen,
            ITokenService token,
            IOptions<JwtSettings> settings,
            IMapper mapper,
            IAppLogger<RegisterPatientHandler> logger)
        {
            _identity = identity;
            _uow = uow;
            _codeGen = codeGen;
            _token = token;
            _settings = settings.Value;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<AuthDto>> Handle(RegisterPatientCommand command)
        {
            var dto = command.Dto;

            _logger.LogInfo("Registering patient with Username: {Username}, IdentityNumber: {IdentityNumber}", dto.Username, dto.IdentityNumber);

            if (await _identity.UsernameExistsAsync(dto.Username))
            {
                _logger.LogWarning("Username already exists: {Username}", dto.Username);
                return Result.Failure<AuthDto>("اسم المستخدم موجود بالفعل");
            }

            if (await _uow.Repository<Patient>().ExistsAsync(p => p.IdentityNumber == dto.IdentityNumber))
            {
                _logger.LogWarning("Identity number already exists: {IdentityNumber}", dto.IdentityNumber);
                return Result.Failure<AuthDto>("رقم الهوية مسجل بالفعل");
            }

            await _uow.BeginTransactionAsync();
            try
            {
                _logger.LogInfo("Creating Identity user for patient: {Username}", dto.Username);

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

                await _identity.AssignRoleAsync(userId, UserRoles.Patient);

                var patient = _mapper.Map<Patient>(dto);
                patient.ApplicationUserId = userId;
                patient.PatientCode = await _codeGen.GenerateAsync("PAT", "PatientCodeSeq");
                patient.CreatedAt = DateTime.UtcNow;

                await _uow.Repository<Patient>().AddAsync(patient);

                var tokens = await _token.GenerateAsync(userId);

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
    }
}
