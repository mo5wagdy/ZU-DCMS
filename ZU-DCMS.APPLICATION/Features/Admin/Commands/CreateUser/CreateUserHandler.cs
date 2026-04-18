using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateUser
{
    public class CreateUserHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IIdentityService _identity;
        private readonly IUserCodeGenerator _codeGen;
        private readonly ILogger<CreateUserHandler> _logger;

        public CreateUserHandler(
            IUnitOfWork uow,
            IIdentityService identity,
            IUserCodeGenerator codeGen,
            ILogger<CreateUserHandler> logger)
        {
            _uow = uow;
            _identity = identity;
            _codeGen = codeGen;
            _logger = logger;
        }

        public async Task<Result<StaffUsersDto>> Handle(CreateUserCommand command)
        {
            var dto = command.Dto;

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

                return Result.Success(result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", dto.Username);

                await _uow.RollbackTransactionAsync();
                
                return Result.Failure<StaffUsersDto>("حدث خطأ أثناء إنشاء المستخدم");
            }
        }
    }
}
