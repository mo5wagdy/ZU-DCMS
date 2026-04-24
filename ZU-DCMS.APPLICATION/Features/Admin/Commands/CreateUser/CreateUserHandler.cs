using MediatR;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<StaffUsersDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;
        private readonly IFusionCache _cache;
        private readonly IUserCodeGenerator _codeGen;
        private readonly ILogger<CreateUserHandler> _logger;

        public CreateUserHandler
        (
            IUnitOfWork uow,
            IIdentityService identity,
            IFusionCache cache,
            IUserCodeGenerator codeGen,
            ILogger<CreateUserHandler> logger
        )
        {
            _uow = uow;
            _identity = identity;
            _cache = cache;
            _codeGen = codeGen;
            _logger = logger;
        }

        public async Task<Result<StaffUsersDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;

            // __ Check user email uniqueness __ //
            if (await _identity.EmailExistsAsync(dto.Email))
            {
                return Result.Failure<StaffUsersDto>("الإيميل موجود بالفعل");
            }

            // __ Check username uniqueness __ //
            if (await _identity.UsernameExistsAsync(dto.Username))
            {
                return Result.Failure<StaffUsersDto>("اسم المستخدم موجود بالفعل");
            }

            // __ Chech phone uniqueness __ //
            var phone = _identity.FindByPhoneAsync(dto.PhoneNumber);
            
            if (phone != null)
            {
                return Result.Failure<StaffUsersDto>("رقم الهاتف موجود بالفعل");
            }

            // __ Patients not allowed __ //
            if (dto.type != UserType.Staff)
            {
                return Result.Failure<StaffUsersDto>("لا يمكن إضافة عيان إلى الطاقم الإداري");
            }

            // __ Check Roles must not be patient __ //
            var roles = await _identity.GetRolesAsync(dto.Role);

            if (roles.Contains(UserRoles.Patient))
            {
                return Result.Failure<StaffUsersDto>("لا يمكن إضافه عيان إلى الأعضاء المسؤولين");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                // __ Create Identity user __ //
                var (success, userId, error) = await _identity.CreateUserAsync
                    (
                        dto.Username,
                        dto.Email,
                        dto.PhoneNumber,
                        dto.FullName,
                        dto.type,
                        dto.Password
                    );

                // __ If failed to create
                if (!success)
                {
                    await _uow.RollbackTransactionAsync();

                    return Result.Failure<StaffUsersDto>(error);
                }

                // __ Adding to the selected role __ //
                await _identity.AssignRoleAsync(userId, dto.Role);

                // __ If the user is student __ //
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

                // __ If the user is intern doctor __ //
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


                // __ Cache version __ //
                var version = await _cache.GetOrSetAsync("students:version", _ => Task.FromResult(1));

                // __ Update Cache Version __ //
                await _cache.SetAsync("students:version", version + 1);

                return Result.Success(result);
            }

            // __ If the transaction failed __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();

                _logger.LogError(ex, "Error creating user {PhoneNumber}", dto.Username);
                
                return Result.Failure<StaffUsersDto>("حدث خطأ أثناء إنشاء المستخدم");
            }
        }
    }
}
