using AutoMapper;
using System.Linq.Expressions;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.Application.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdentityService _identity;
    private readonly IMapper _mapper;
    private readonly IAppLogger<PatientService> _logger;

    public PatientService(
        IUnitOfWork uow,
        IIdentityService identity,
        IMapper mapper,
        IAppLogger<PatientService> logger)
    {
        _uow = uow;
        _identity = identity;
        _mapper = mapper;
        _logger = logger;
    }

    // __ Since the patient is linked to the user, we can get the patient by user id or by patient id. __ // 
    
    // ________________ Get By Patient Id ________________ //
    public async Task<Result<PatientDto>> GetByIdAsync(int id)
    {
        _logger.LogInfo("Fetching patient by ID: {Id}", id);

        var patient = await _uow.Repository<Patient>().GetByIdAsync(id);

        if (patient is null)
        {
            _logger.LogWarning("Patient not found with ID: {Id}", id);
            
            return Result.Failure<PatientDto>("المريض غير موجود");
        }
        
        _logger.LogInfo("Patient found: {FullName} (ID: {Id})", patient.FullName, patient.Id);

        return Result.Success<PatientDto>(_mapper.Map<PatientDto>(patient));
    }

    // ________________ Get By Identity User Id ________________ //
    public async Task<Result<PatientDto>> GetByUserIdAsync(string userId)
    {
        _logger.LogInfo("Fetching patient by User ID: {UserId}", userId);

        var patient = await _uow.Repository<Patient>().GetFirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        if (patient is null)
        {
            _logger.LogWarning("Patient not found with User ID: {UserId}", userId);
            return Result.Failure<PatientDto>("المريض غير موجود");
        }
        
        _logger.LogInfo("Patient found: {FullName} (User ID: {UserId})", patient.FullName, userId);

        return Result.Success<PatientDto>(_mapper.Map<PatientDto>(patient));
    }

    // ________________ Update Profile ________________ //
    public async Task<Result<UpdatePatientDto>> UpdateProfileAsync(int id, UpdatePatientDto dto)
    {
        _logger.LogInfo("Fetching patient profile for update, ID: {Id}", id);
        
        var patient = await _uow.Repository<Patient>().GetByIdAsync(id);  
        
        if(patient is null)
        {
            _logger.LogWarning("Patient not found for update, ID: {Id}", id);

            return Result.Failure<UpdatePatientDto>("المريض غير موجود");
        }

        // __ We use transaction here because we might need to update the username in the identity service, and if that fails, we don't want to update the patient data. __ //
        await _uow.BeginTransactionAsync();

        try
        {
            _logger.LogInfo("Updating patient profile, ID: {Id}", id);

            // ________________ Username Change ________________ //

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

            // ________________ Remaining Fields ________________ //

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

            // __ Save Changes and Commit Transaction __ //
            await _uow.SaveChangesAsync();
            
            await _uow.CommitTransactionAsync();

            _logger.LogInfo("Patient profile updated successfully, ID: {Id}", id);

            return Result.Success<UpdatePatientDto>(_mapper.Map<UpdatePatientDto>(patient));
        }
        catch
        {
            // __ Rollback Transaction on Error __ //
            await _uow.RollbackTransactionAsync();

            _logger.LogError("An error occurred while updating patient profile");

            return Result.Failure<UpdatePatientDto>("حدث خطأ ، حاول مرة أخرى");
        }
    }

    // ________________ Get All (Admin) ________________ //
    public async Task<Result<PagedResult<PatientDto>>> GetAllAsync(PagedRequest request)
    {
        _logger.LogInfo("Fetching patients with pagination. Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortDescending: {SortDescending}, SearchTerm: {SearchTerm}");

        // ________________ Search ________________ //
        Expression<Func<Patient, bool>> filter = p => 
            string.IsNullOrWhiteSpace(request.SearchTerm) ||
            p.FullName.Contains(request.SearchTerm) ||
            p.PatientCode.Contains(request.SearchTerm) ||
            p.PhoneNumber.Contains(request.SearchTerm);
        
        // ________________ Sorting ________________ //
        Func<IQueryable<Patient>, IOrderedQueryable<Patient>> orderBy = q =>
        {
            var sortBy = request.SortBy?.ToLower();
            if (request.SortDescending)
            {
                return sortBy switch
                {
                    "name" => q.OrderByDescending(p => p.FullName),
                    "code" => q.OrderByDescending(p => p.PatientCode),
                    _ => q.OrderByDescending(p => p.CreatedAt)
                };
            }
            return sortBy switch
            {
                "name" => q.OrderBy(p => p.FullName),
                "code" => q.OrderBy(p => p.PatientCode),
                _ => q.OrderBy(p => p.CreatedAt)
            };
        };

        // ________________ Pagination ________________ //
        var (items, totalCount) = await _uow.Repository<Patient>().GetPagedListAsync(
            skip: (request.Page - 1) * request.PageSize,
            take: request.PageSize,
            predicate: filter,
            orderBy: orderBy,
            disabledTracking: true 
        );

        var dtos = _mapper.Map<List<PatientDto>>(items);

        var pagedResult = PagedResult<PatientDto>.Create(dtos, totalCount, request);

        _logger.LogInfo("Fetched {Count} patients out of {TotalCount}. Page: {Page}/{TotalPages}", dtos.Count, totalCount, pagedResult.Page, pagedResult.TotalPages);
        
        return Result.Success<PagedResult<PatientDto>>(pagedResult);
    }
}
