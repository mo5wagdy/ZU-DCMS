using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patients.Commands.UpdateProfile
{
    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result<PatientDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;
        private readonly IMapper _mapper;
        private readonly IAppLogger<UpdateProfileHandler> _logger;

        public UpdateProfileHandler(
            IUnitOfWork uow,
            IIdentityService identity,
            IMapper mapper,
            IAppLogger<UpdateProfileHandler> logger)
        {
            _uow = uow;
            _identity = identity;
            _mapper = mapper;
            _logger = logger;
        }

        // ________________ Update Profile ________________ //
        public async Task<Result<PatientDto>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var id = command.Id;
           
            var dto = command.Dto;

            _logger.LogInfo("Fetching patient profile for update, ID: {Id}", id);
            
            // __ Checking Patient Availability __ //
            var patient = await _uow.Repository<Patient>().GetByIdAsync(id);  
            
            if(patient is null)
            {
                _logger.LogWarning("Patient not found for update, ID: {Id}", id);
                
                return Result.Failure<PatientDto>("Patient not found");
            }

            // __ We use transaction here because we might need to update the username in the identity service, and if that fails, we don't want to update the patient data. __ //
            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Updating patient profile, ID: {Id}", id);

                if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    var newUsername = dto.Username.Trim().ToLower();

                    var user = await _identity.FindByIdAsync(patient.ApplicationUserId);

                    // __ Checking if username already exists 
                    var isSame = user?.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase) ?? false;

                    if (!isSame)
                    {
                        if (await _identity.UsernameExistsAsync(newUsername))
                        {
                            await _uow.RollbackTransactionAsync();
                            
                            _logger.LogWarning("Username already exists: {PhoneNumber}", newUsername);
                            
                            return Result.Failure<PatientDto>("Username already exists");
                        }

                        _logger.LogInfo("Updating username for patient ID: {Id}, New Username: {Username}", id, newUsername);

                        // __ Updating username __ //
                        var updated = await _identity.UpdateUsernameAsync(patient.ApplicationUserId,newUsername);

                        if (!updated)
                        {
                            await _uow.RollbackTransactionAsync();
                        
                            _logger.LogWarning("Failed to update username: {Username}", newUsername);
                            
                            return Result.Failure<PatientDto>("Failed to update username");
                        }
                    }
                }

                _logger.LogInfo("Updating remaining profile fields for patient ID: {Id}", id);

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    patient.PhoneNumber = dto.PhoneNumber.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    patient.Email = dto.Email.Trim();

                if (dto.ChronicConditions.HasValue)
                    patient.ChronicConditions = dto.ChronicConditions.Value;

                if (!string.IsNullOrWhiteSpace(dto.OtherConditions))
                    patient.OtherConditions = dto.OtherConditions.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Address))
                    patient.Address = dto.Address.Trim();

                patient.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInfo("Updating patient entity in repository for patient ID: {Id}", id);

                _uow.Repository<Patient>().Update(patient);

                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);
                
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Patient profile updated successfully, ID: {Id}", id);

                return Result.Success<PatientDto>(_mapper.Map<PatientDto>(patient));
            }

            catch
            {
                await _uow.RollbackTransactionAsync();
                
                _logger.LogError("An error occurred while updating patient profile");
                
                return Result.Failure<PatientDto>("An error occurred, try again");
            }
        }
    }
}
