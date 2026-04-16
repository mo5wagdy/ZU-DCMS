using AutoMapper;
using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Services.Implementations
{
    internal class AdminService : IAdminService
    {

        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;
        private readonly ICacheService _cache;
        private readonly IUserCodeGenerator _codeGen;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IUnitOfWork uow,
            IIdentityService identity,
            ICacheService cache,
            IUserCodeGenerator codeGen,
            IMapper mapper,
            ILogger<AdminService> logger)
        {
            _uow = uow;
            _identity = identity;
            _cache = cache;
            _codeGen = codeGen;
            _mapper = mapper;
            _logger = logger;
        }

        // =========================
        // USERS
        // =========================

        public async Task<Result<PagedResult<StaffUsersDto>>> GetAllUsersAsync(PagedRequest request, string role)
        {
            /*
            1. Build base query from Users table
            2. If role != null → filter by role
            3. Apply search (if request.Search exists)
            4. Apply pagination (Skip / Take)
            5. Include roles + status
            6. Map to StaffUsersDto
            7. Return PagedResult (Items + TotalCount)
            */

            var appUsers = _identity.GetAllUsersAsync(request, role);

            if(appUsers is null)
            {
                return Result.Failure<PagedResult<StaffUsersDto>>("خطأ في تحميل المستخدمين");
            }

            return Result.Success<PagedResult<StaffUsersDto>>(appUsers.Result);
        }

        public async Task<Result<StaffUsersDto>> GetUserByIdAsync(string userId)
        {
            /*
            1. Fetch user by Id
            2. If null → return null
            3. Map to StaffUsersDto
            4. Return DTO
            */

            var user = _identity.FindByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<StaffUsersDto>("المستخدم غير موجود");
            }

            return Result.Success<StaffUsersDto>(_mapper.Map<StaffUsersDto>(user));
        }

        public async Task<Result<StaffUsersDto>> CreateUserAsync(CreateUserDto dto)
        {
            /*
            1. Validate DTO (email, password, role)
            2. Check if email already exists
                → if yes: throw ConflictException

            3. Create IdentityUser
            4. Assign role
            5. Save user to DB

            6. Optional:
                - Send welcome email
                - Log admin action

            7. Map to StaffUsersDto
            8. Return result
            */

            // Check email uniqueness
            if (await _identity.EmailExistsAsync(dto.Email))
            {
                return Result.Failure<StaffUsersDto>("الإيميل موجود بالفعل");
            }

            // Check username uniqueness
            if (await _identity.UsernameExistsAsync(dto.Username))
            {
                return Result.Failure<StaffUsersDto>("اسم المستخدم موجود بالفعل");
            }

            // Check Roles
            var roles = await _identity.GetRolesAsync(dto.Role);

            if (roles.Contains(UserRoles.Patient))
            {
                return Result.Failure<StaffUsersDto>("لا يمكن إضافه عيان إلى الأعضاء المسؤولين");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                // Create Identity user
                var (success, userId, error) = await _identity.CreateUserAsync
                    (
                        dto.Username,
                        dto.Email,
                        dto.FullName,
                        dto.Password
                    );

                if (!success)
                {
                    await _uow.RollbackTransactionAsync();

                    return Result.Failure<StaffUsersDto>(error);
                }

                await _identity.AssignRoleAsync(userId, dto.Role);

                if (dto.Role == UserRoles.Student)
                {
                    var student = new Student
                    {
                        ApplicationUserId = userId,
                        StudentCode = await _codeGen.GenerateAsync("STU", "StudentCodeSeq"),
                        FullName = dto.FullName.Trim(),
                        AcademicYear = dto.AcademicYear ?? 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<Student>().AddAsync(student);
                }

                else if (dto.Role == UserRoles.InternDoctor)
                {
                    var intern = new InternDoctor
                    {
                        ApplicationUserId = userId,
                        DoctorCode = await _codeGen.GenerateAsync("IND", "InternDoctorCodeSeq"),
                        FullName = dto.FullName.Trim(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<InternDoctor>().AddAsync(intern);
                }

                await _uow.CommitTransactionAsync(userId);

                var user = await _identity.FindByIdAsync(userId);

                var result = new StaffUsersDto
                {
                    Id = userId,
                    FullName = dto.FullName,
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                //email

                return Result.Success(result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", dto.Username);

                await _uow.RollbackTransactionAsync();
                
                return Result.Failure<StaffUsersDto>("حدث خطأ أثناء إنشاء المستخدم");
            }
        }

        public async Task ToggleUserActiveAsync(string userId)
        {
            /*
            1. Fetch user by Id
            2. If null → NotFound
            3. Toggle IsActive flag:
                - true → false
                - false → true

            4. SaveChanges

            5. If deactivated:
                - revoke sessions/tokens
                - optional cache invalidation

            6. Log admin action
            */

            throw new NotImplementedException();
        }

        // =========================
        // SYSTEM CONFIG
        // =========================

        public async Task<List<SystemConfigDto>> GetAllConfigsAsync()
        {
            /*
            1. Fetch all configs from SystemConfig table
            2. Map to DTO
            3. Return list
            */

            throw new NotImplementedException();
        }

        public async Task UpdateConfigAsync(string key, string value, string adminId)
        {
            /*
            1. Fetch config by key
            2. If not found → NotFound
            3. Update value
            4. Set UpdatedBy = adminId
            5. SaveChanges

            6. Invalidate config cache
            7. Log admin change
            */

            throw new NotImplementedException();
        }

        // =========================
        // TERM
        // =========================

        public async Task<List<TermDto>> GetAllTermsAsync()
        {
            /*
            1. Fetch all terms
            2. Order by StartDate desc
            3. Map to DTO
            */

            throw new NotImplementedException();
        }

        public async Task<TermDto?> GetTermByIdAsync(int termId)
        {
            /*
            1. Fetch term by Id
            2. If null → return null
            3. Include requirements + stats
            4. Map to DTO
            */

            throw new NotImplementedException();
        }

        public async Task<TermDto> CreateTermAsync(CreateTermDto dto, string adminId)
        {
            /*
            1. Validate date range (Start < End)
            2. Ensure no overlapping active term (business rule)
            3. Create Term entity
            4. Set CreatedBy = adminId
            5. SaveChanges

            6. Return mapped DTO
            */

            throw new NotImplementedException();
        }

        public async Task<TermDto> UpdateTermAsync(int termId, UpdateTermDto dto, string adminId)
        {
            /*
            1. Fetch term
            2. If null → NotFound
            3. Update allowed fields only
            4. Set UpdatedBy = adminId
            5. SaveChanges
            6. Return DTO
            */

            throw new NotImplementedException();
        }

        public async Task SetActiveTermAsync(int termId, string adminId)
        {
            /*
            1. Fetch term
            2. If null → NotFound

            3. Deactivate current active term (if any)
            4. Set selected term as Active

            5. SaveChanges

            6. Invalidate caches:
                - student progress
                - dashboards

            7. Log admin action
            */

            throw new NotImplementedException();
        }

        // =========================
        // STUDENT REQUIREMENTS
        // =========================

        public async Task SetStudentRequirementsAsync(int studentId, int termId, List<SetRequirementDto> requirements, string adminId)
        {
            /*
            1. Validate student exists
            2. Validate term exists
            3. Fetch existing requirements for (studentId + termId)

            4. Remove old requirements
            5. Insert new requirements

            6. SaveChanges

            7. Invalidate cache:
                - StudentRequirements(studentId, termId)

            8. Log admin action
            */

            throw new NotImplementedException();
        }

        public async Task<List<StudentRequirementDto>> GetStudentRequirementsAsync(int studentId, int termId)
        {
            /*
            1. Fetch requirements where studentId + termId
            2. Include related metadata (Case types, counts)
            3. Map to DTO
            4. Return list
            */

            throw new NotImplementedException();
        }

        public async Task TransferRequirementsAsync(int studentId, int fromTermId, int toTermId, string adminId)
        {
            /*
            1. Fetch requirements from old term
            2. If empty → return or throw

            3. Validate target term exists
            4. Map old requirements → new term requirements

            5. Remove old if business rule requires OR keep history

            6. SaveChanges

            7. Invalidate caches:
                - both terms student requirements

            8. Log admin transfer action
            */

            throw new NotImplementedException();
        }
    }
}