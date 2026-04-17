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

                //notify & email

                return Result.Success(result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", dto.Username);

                await _uow.RollbackTransactionAsync();
                
                return Result.Failure<StaffUsersDto>("حدث خطأ أثناء إنشاء المستخدم");
            }
        }

        public async Task<Result> ToggleUserActiveAsync(string userId)
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
            //var toggled = await _identity.ToggleActiveAsync(userId);

            return /*toggled?*/ Result.Success(); //: Result.Failure("المستخدم غير موجود");
        }
        

        // =========================
        // SYSTEM CONFIG
        // =========================

        public async Task<Result<List<SystemConfigDto>>> GetAllConfigsAsync()
        {
            /*
            1. Fetch all configs from SystemConfig table
            2. Map to DTO
            3. Return list
            */

            var configs = await _uow.Repository<SystemConfig>().GetListAsync();

            return Result.Success(_mapper.Map<List<SystemConfigDto>>(configs));

        }

        public async Task<Result> UpdateConfigAsync(string key, string value, string adminId)
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
            var config = await _uow.Repository<SystemConfig>().GetFirstOrDefaultAsync(c => c.Key == key);

            if (config is null)
                return Result.Failure("الإعداد غير موجود");

            config.Value = value.Trim();
            config.UpdatedByAdminId = adminId;
            config.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<SystemConfig>().Update(config);
           
            await _uow.SaveChangesAsync(adminId);

            // CACHE SETTINGS

            return Result.Success();

        }

        // =========================
        // TERM
        // =========================
        public async Task<Result<List<TermDto>>> GetAllTermsAsync()
        {
            /*
            1. Fetch all terms
            2. Order by StartDate desc
            3. Map to DTO
            */

            var terms = await _uow.Repository<Term>().GetListAsync();

            var ordered = terms.OrderByDescending(t => t.StartDate);

            return Result.Success(_mapper.Map<List<TermDto>>(ordered));
        }

        public async Task<Result<TermDto>> GetTermByIdAsync(int termId)
        {
            /*
            1. Fetch term by Id
            2. If null → return null
            3. Include requirements + stats
            4. Map to DTO
            */

            var term = await _uow.Repository<Term>().GetByIdAsync(termId);

            if (term is null)
                return Result.Failure<TermDto>("الترم غير موجود");

            return Result.Success(_mapper.Map<TermDto>(term));
        }

        public async Task<Result<TermDto>> CreateTermAsync(CreateTermDto dto, string adminId)
        {
            /*
            1. Validate date range (Start < End)
            2. Ensure no overlapping active term (business rule)
            3. Create Term entity
            4. Set CreatedBy = adminId
            5. SaveChanges

            6. Return mapped DTO
            */
            // Prevent multiple active terms
            var hasActiveTerm = await _uow.Repository<Term>().ExistsAsync(t => t.IsActive);

            if (hasActiveTerm)
                return Result.Failure<TermDto>("يوجد ترم نشط بالفعل، أوقفه أولاً");

            var term = new Term
            {
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                RequiredCasesCount = dto.RequiredCasesCount,
                IsActive = false,
                CreatedByAdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<Term>().AddAsync(term);
            
            await _uow.SaveChangesAsync(adminId);

            return Result.Success(_mapper.Map<TermDto>(term));


        }

        public async Task<Result<TermDto>> UpdateTermAsync(int termId, UpdateTermDto dto, string adminId)
        {
            /*
            1. Fetch term
            2. If null → NotFound
            3. Update allowed fields only
            4. Set UpdatedBy = adminId
            5. SaveChanges
            6. Return DTO
            */

            var term = await _uow.Repository<Term>().GetByIdAsync(termId);

            if (term is null)
                return Result.Failure<TermDto>("الترم غير موجود");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                term.Name = dto.Name.Trim();

            if (dto.StartDate.HasValue)
                term.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                term.EndDate = dto.EndDate.Value;

            if (dto.RequiredCasesCount.HasValue)
                term.RequiredCasesCount = dto.RequiredCasesCount.Value;

            term.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<Term>().Update(term);
            
            await _uow.SaveChangesAsync(adminId);

            return Result.Success(_mapper.Map<TermDto>(term));
        }

        public async Task<Result> SetActiveTermAsync(int termId, string adminId)
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


            var term = await _uow.Repository<Term>().GetByIdAsync(termId);

            if (term is null)
                return Result.Failure("الترم غير موجود");

            await _uow.BeginTransactionAsync();

            try
            {
                // Deactivate current active term
                var currentActive = await _uow.Repository<Term>().GetFirstOrDefaultAsync(t => t.IsActive);

                if (currentActive != null)
                {
                    currentActive.IsActive = false;

                    currentActive.UpdatedAt = DateTime.UtcNow;

                    _uow.Repository<Term>().Update(currentActive);
                }

                // Activate new term
                term.IsActive = true;

                term.UpdatedAt = DateTime.UtcNow;

                _uow.Repository<Term>().Update(term);

                // Update all active students to new term
                var students = await _uow.Repository<Student>().GetListAsync(s => s.IsActive);

                foreach (var student in students)
                {
                    student.ActiveTermId = termId;

                    student.UpdatedAt = DateTime.UtcNow;

                    _uow.Repository<Student>().Update(student);
                }

                await _uow.CommitTransactionAsync(adminId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error setting active term {TermId}", termId);

                await _uow.RollbackTransactionAsync();
               
                return Result.Failure("حدث خطأ أثناء تفعيل الترم");
            }
        }

        // =========================
        // STUDENT REQUIREMENTS
        // =========================

        public async Task<Result> SetStudentRequirementsAsync(int studentId, int termId, List<SetRequirementDto> requirements, string adminId)
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

            // Verify student exists
            var student = await _uow.Repository<Student>().GetByIdAsync(studentId);

            if (student is null)
                return Result.Failure("الطالب غير موجود");

            // Verify term exists
            var term = await _uow.Repository<Term>().GetByIdAsync(termId);

            if (term is null)
                return Result.Failure("الترم غير موجود");

            // Verify all clinics exist
            var clinicIds = requirements.Select(r => r.ClinicId).ToList();
            
            var clinics = await _uow.Repository<Clinic>().GetListAsync(c => clinicIds.Contains(c.Id) && c.IsActive);

            if (clinics.Count != clinicIds.Distinct().Count())
                return Result.Failure("بعض العيادات غير موجودة أو غير نشطة");

            await _uow.BeginTransactionAsync();

            try
            {
                // Load existing requirements for this student + term
                var existing = await _uow.Repository<TermRequirement>().GetListAsync
                    (
                        r =>
                        r.StudentId == studentId &&
                        r.TermId == termId
                    );

                // Soft delete existing requirements
                foreach (var req in existing)
                    _uow.Repository<TermRequirement>().SoftDelete(req);

                // Create new requirements
                var newRequirements = requirements.Select(r =>
                    new TermRequirement
                    {
                        StudentId = studentId,
                        TermId = termId,
                        ClinicId = r.ClinicId,
                        RequiredCount = r.RequiredCount,
                        CompletedCount = 0,
                        CreatedAt = DateTime.UtcNow
                    });

                await _uow.Repository<TermRequirement>().AddRangeAsync(newRequirements);

                await _uow.CommitTransactionAsync(adminId);

                // Invalidate student progress cache

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error setting requirements for student {StudentId}",
                    studentId);

                await _uow.RollbackTransactionAsync();
                return Result.Failure("حدث خطأ أثناء تحديث المتطلبات");
            }
        }

        public async Task<Result<List<StudentRequirementDto>>> GetStudentRequirementsAsync(int studentId, int termId)
        {
            /*
            1. Fetch requirements where studentId + termId
            2. Include related metadata (Case types, counts)
            3. Map to DTO
            4. Return list
            */

            var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                (
                    r => r.StudentId == studentId &&
                         r.TermId == termId,
                         includes : r => r.Clinic
                );

            return Result.Success(_mapper.Map<List<StudentRequirementDto>>(requirements));
        }

        public async Task<Result> TransferRequirementsAsync(int studentId, int fromTermId, int toTermId, string adminId)
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

            var fromRequirements = await _uow.Repository<TermRequirement>()
            .GetListAsync(r =>
                r.StudentId == studentId &&
                r.TermId == fromTermId &&
                !r.IsSatisfied);

            if (!fromRequirements.Any())
                return Result.Failure("لا توجد متطلبات غير مكتملة للترحيل");

            var toTerm = await _uow.Repository<Term>().GetByIdAsync(toTermId);

            if (toTerm == null)
                return Result.Failure("الترم المحول إليه غير موجود");

            await _uow.BeginTransactionAsync();
            
            try
            {
                foreach (var req in fromRequirements)
                {
                    // Check if requirement already exists in target term
                    var exists = await _uow.Repository<TermRequirement>()
                        .ExistsAsync(r =>
                            r.StudentId == studentId &&
                            r.TermId == toTermId &&
                            r.ClinicId == req.ClinicId);

                    if (exists) continue;

                    // Transfer remaining count to new term
                    var remaining = req.RequiredCount
                        - req.CompletedCount
                        - req.TransferredCount;

                    var transferred = new TermRequirement
                    {
                        StudentId = studentId,
                        TermId = toTermId,
                        ClinicId = req.ClinicId,
                        RequiredCount = remaining,
                        TransferredCount = remaining,
                        CompletedCount = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<TermRequirement>()
                        .AddAsync(transferred);
                }

                await _uow.CommitTransactionAsync(adminId);

                // CACHE SETTINGS 
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring requirements for student {StudentId}",  studentId);

                await _uow.RollbackTransactionAsync();
               
                return Result.Failure("حدث خطأ أثناء ترحيل المتطلبات");
            }
        }
    }
}