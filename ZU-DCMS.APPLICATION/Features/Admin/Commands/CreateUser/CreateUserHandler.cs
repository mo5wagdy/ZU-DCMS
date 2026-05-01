using MediatR;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
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
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber)) 
            {
                return Result.Failure<StaffUsersDto>("رقم الهاتف مطلوب");
            }

            var phone = await _identity.FindByPhoneAsync(dto.PhoneNumber);

            if (phone != null)
            {
                return Result.Failure<StaffUsersDto>("رقم الهاتف موجود بالفعل");
            }

            // __ Patients not allowed __ //
            if (dto.Type != UserType.Staff)
            {
                return Result.Failure<StaffUsersDto>("لا يمكن إضافة عيان إلى الطاقم الإداري");
            }

            // __ We don't need to check roles here since we trust dto.Role string from CreatableRoles __ //
            var selectedRole = dto.Role;

            if (selectedRole == UserRoles.Patient)
            {
                return Result.Failure<StaffUsersDto>("لا يمكن إضافه عيان إلى الأعضاء المسؤولين");
            }

            // __ Prevent assigning student role to more than 5 academic years __ //
            if (dto.AcademicYear > 5)
            {
                return Result.Failure<StaffUsersDto>("عدد السنوات الأكاديمية الرسمية 5 سنوات");
            }


            await _uow.BeginTransactionAsync();

            try
            {
                // __ Generate a random password if not provided __ //
                var password = string.IsNullOrWhiteSpace(dto.Password) ? "ZU-DCMS@2026" : dto.Password;

                // __ Create Identity user __ //
                var (success, userId, error) = await _identity.CreateUserAsync
                    (
                       username: dto.Username,
                       email: dto.Email,
                       phoneNumber: dto.PhoneNumber,
                       fullName: dto.FullName,
                       type: dto.Type,
                       password: password
                    );

                // __ If failed to create
                if (!success)
                {
                    await _uow.RollbackTransactionAsync();

                    return Result.Failure<StaffUsersDto>(error ?? "فشل إنشاء المستخدم");
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
                        DoctorCode = await _codeGen.GenerateAsync("IND", "DoctorCodeSeq"),
                        FullName = dto.FullName.Trim(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<InternDoctor>().AddAsync(intern);
                }

                // __ If the user is teaching assistant __ //
                else if (dto.Role == UserRoles.TeachingAssistant)
                {
                    var ta = new TeachingAssistant
                    {
                        ApplicationUserId = userId,
                        TACode = await _codeGen.GenerateAsync("TEA", "TACodeSeq"),
                        FullName = dto.FullName.Trim(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<TeachingAssistant>().AddAsync(ta);
                }

                await _uow.CommitTransactionAsync(userId);

                var user = await _identity.FindByIdAsync(userId);

                var result = new StaffUsersDto
                {
                    Id = userId,
                    FullName = dto.FullName,
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = dto.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                result.Role = dto.Role;


                //notify & email


                // __ Update Staff Cache Version __ //
                var staffVersion = await _cache.GetOrSetAsync(CacheKeys.StaffUsersVersion, _ => Task.FromResult(1));
                await _cache.SetAsync(CacheKeys.StaffUsersVersion, staffVersion + 1);

                // __ Update Student Cache Version if student __ //
                if (dto.Role == UserRoles.Student)
                {
                    var version = await _cache.GetOrSetAsync("students:version", _ => Task.FromResult(1));
                    await _cache.SetAsync("students:version", version + 1);
                }

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