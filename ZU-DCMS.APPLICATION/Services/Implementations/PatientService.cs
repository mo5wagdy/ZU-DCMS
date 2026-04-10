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

    public PatientService(
        IUnitOfWork uow,
        IIdentityService identity,
        IMapper mapper)
    {
        _uow = uow;
        _identity = identity;
        _mapper = mapper;
    }

    // __ Since the patient is linked to the user, we can get the patient by user id or by patient id. __ // 
    
    // ________________ Get By Id ________________ //
    public async Task<PatientResult> GetByIdAsync(int id)
    {
        var patient = await _uow.Repository<Patient>().GetByIdAsync(id);

        if (patient is null) 
            return PatientResult.Fail("المريض غير موجود");
        
        return PatientResult.Success(_mapper.Map<PatientDto>(patient));
    }

    // ________________ Get By UserId ________________ //

    public async Task<PatientResult> GetByUserIdAsync(string userId)
    {
        var patient = await _uow.Repository<Patient>().GetFirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        if (patient is null) 
            return PatientResult.Fail("المريض غير موجود"); 
        
        return PatientResult.Success(_mapper.Map<PatientDto>(patient));
    }

    // ________________ Update Profile ________________ //

    public async Task<PatientResult> UpdateProfileAsync(int id, UpdatePatientDto dto)
    {
        
        var patient = await _uow.Repository<Patient>().GetByIdAsync(id) ?? throw new KeyNotFoundException("المريض غير موجود");
        
        // __ We use transaction here because we might need to update the username in the identity service, and if that fails, we don't want to update the patient data. __ //
        await _uow.BeginTransactionAsync();

        try
        {

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
                        return PatientResult.Fail("اسم المستخدم موجود بالفعل");
                    }

                    var updated = await _identity.UpdateUsernameAsync(patient.ApplicationUserId,newUsername);

                    if (!updated)
                    {
                        await _uow.RollbackTransactionAsync();
                        return PatientResult.Fail("فشل تحديث اسم المستخدم");
                    }
                }
            }

            // ________________ Remaining Fields ________________ //

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

            _uow.Repository<Patient>().Update(patient);

            // __ Save Changes and Commit Transaction __ //
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();

            return PatientResult.Success(_mapper.Map<PatientDto>(patient));
        }
        catch
        {
            // __ Rollback Transaction on Error __ //
            await _uow.RollbackTransactionAsync();
            throw;
        }
    }

    // ________________ Get All (Admin) ________________ //

    public async Task<PagedResult<PatientDto>> GetAllAsync(PagedRequest request)
    {
        // ________________ Search ________________ //
        // E
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

        return PagedResult<PatientDto>.Create(dtos, totalCount, request);
    }
}
