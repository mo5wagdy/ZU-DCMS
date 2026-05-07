using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Auth;
using ZU_DCMS.APPLICATION.Common.Extensions;
using ZU_DCMS.APPLICATION.Common.Token;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Features.Auth.Commands.RegisterPatient
{
    public class RegisterPatientHandler : IRequestHandler<RegisterPatientCommand, Result<AuthDto>>
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

        public async Task<Result<AuthDto>> Handle(RegisterPatientCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;

            // __ Normalize Inputs __ //
            //dto.Username = dto.Username.Trim().ToLower().NormalizeDigits();
            dto.PhoneNumber = dto.PhoneNumber.Trim().NormalizeDigits();
            dto.IdentityNumber = dto.IdentityNumber.Trim().NormalizeDigits();

            _logger.LogInfo("Registering patient with PhoneNumber: {PhoneNumber}, IdentityNumber: {IdentityNumber}", dto.PhoneNumber, dto.IdentityNumber);


            // __ Check identity number uniqueness __ //
            if (await _uow.Repository<Patient>().ExistsAsync(p => p.IdentityNumber == dto.IdentityNumber))
            {
                _logger.LogWarning("Identity number already exists: {IdentityNumber}", dto.IdentityNumber);
                
                return Result.Failure<AuthDto>("National ID is already registered");
            }

            if (await _uow.Repository<Patient>().ExistsAsync(p => p.PhoneNumber == dto.PhoneNumber))
            {
                _logger.LogWarning("Phone number already exists: {PhoneNumber}", dto.PhoneNumber);

                return Result.Failure<AuthDto>("Phone number is already registered");
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && await _identity.EmailExistsAsync(dto.Email))
            {
                _logger.LogWarning("Email already exists: {Email}", dto.Email);

                return Result.Failure<AuthDto>("Email is already registered");
            }

            if (dto.Type != UserType.Patient)
            {
                _logger.LogWarning("Attempt Registering With Different Type Which Is Not Allowed");

                return Result.Failure<AuthDto>("Not allowed");
            }

            // __ Begin transaction — Identity + Patient must both succeed __ //
            await _uow.BeginTransactionAsync();
           
            try
            {
                _logger.LogInfo("Creating Identity user for patient: {PhoneNumber}", dto.PhoneNumber);

                // __ Create Identity user — password is the identity number __ //
                var (success, userId, error) = await _identity.CreateUserAsync
                (
                    username: Guid.NewGuid().ToString(),
                    phoneNumber: dto.PhoneNumber,
                    email: dto.Email ?? null,
                    fullName: dto.FullName,
                    type : dto.Type,
                    password : dto.IdentityNumber
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

                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);
                
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Patient registration successful: {PhoneNumber}", dto.PhoneNumber);

                return Result.Success<AuthDto>(new AuthDto
                {
                    AccessToken = tokens.Value.AccessToken,
                    RefreshToken = tokens.Value.RefreshToken,
                    UserId = userId,
                    FullName = dto.FullName,
                    Role = UserRoles.Patient,
                    RedirectUrl = RedirectUrls.GetByRole(UserRoles.Patient)
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
               
                _logger.LogError("An error occurred during patient registration");
                
                return Result.Failure<AuthDto>("An error occurred during registration, try again");
            }
        }
    }
}