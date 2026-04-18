using AutoMapper;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patient.Commands.UpdateProfile
{
    public class UpdateProfileHandler
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

        public async Task<Result<UpdatePatientDto>> Handle(UpdateProfileCommand command)
        {
            var id = command.Id;
            var dto = command.Dto;

            _logger.LogInfo("Fetching patient profile for update, ID: {Id}", id);
            
            var patient = await _uow.Repository<Domain.Entities.Patient>().GetByIdAsync(id);  
            
            if(patient is null)
            {
                _logger.LogWarning("Patient not found for update, ID: {Id}", id);
                return Result.Failure<UpdatePatientDto>("المريض غير موجود");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                _logger.LogInfo("Updating patient profile, ID: {Id}", id);

                if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    var newUsername = dto.Username.Trim();

                    var user = await _identity.FindByIdAsync(patient.ApplicationUserId);

                    var isSame = user?.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase) ?? false;

                    if (!isSame)
                    {
                        if (await _identity.UsernameExistsAsync(newUsername))
                        {
                            await _uow.RollbackTransactionAsync();
                            _logger.LogWarning("Username already exists: {Username}", newUsername);
                            return Result.Failure<UpdatePatientDto>("اسم المستخدم موجود بالفعل");
                        }

                        _logger.LogInfo("Updating username for patient ID: {Id}, New Username: {Username}", id, newUsername);

                        var updated = await _identity.UpdateUsernameAsync(patient.ApplicationUserId,newUsername);

                        if (!updated)
                        {
                            await _uow.RollbackTransactionAsync();
                            _logger.LogWarning("Failed to update username: {Username}", newUsername);
                            return Result.Failure<UpdatePatientDto>("فشل تحديث اسم المستخدم");
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

                _uow.Repository<Domain.Entities.Patient>().Update(patient);

                await _uow.SaveChangesAsync();
                
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Patient profile updated successfully, ID: {Id}", id);

                return Result.Success<UpdatePatientDto>(_mapper.Map<UpdatePatientDto>(patient));
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("An error occurred while updating patient profile");
                return Result.Failure<UpdatePatientDto>("حدث خطأ ، حاول مرة أخرى");
            }
        }
    }
}
