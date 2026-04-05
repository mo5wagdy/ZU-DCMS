using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                   .NotEmpty().WithMessage("اسم المستخدم مطلوب")
                   .MinimumLength(3).WithMessage("اسم المستخدم لازم يكون 3 حروف على الأقل")
                   .MaximumLength(50).WithMessage("اسم المستخدم لازم يكون أقل من 50 حرف");

            RuleFor(x => x.FullName)
                   .NotEmpty().WithMessage("الاسم مطلوب")
                   .MaximumLength(100).WithMessage("الاسم لازم يكون أقل من 100 حرف");

            RuleFor(x => x.Email)
                   .NotEmpty().WithMessage("الإيميل مطلوب")
                   .EmailAddress().WithMessage("الإيميل غير صحيح");

            RuleFor(x => x.Role)
                   .NotEmpty().WithMessage("الدور مطلوب")
                   .Must(x => new[]
                   {
                   UserRoles.Student,
                   UserRoles.InternDoctor,
                   UserRoles.Admin
                   }.Contains(x))
                   .WithMessage("الدور غير صحيح");

            RuleFor(x => x.StudentCode)
                   .NotEmpty().WithMessage("كود الطالب مطلوب")
                   .When(x => x.Role == UserRoles.Student);

            RuleFor(x => x.AcademicYear)
                   .GreaterThan(0).WithMessage("السنة الدراسية مطلوبة")
                   .When(x => x.Role == UserRoles.Student);

            RuleFor(x => x.DoctorCode)
                   .NotEmpty().WithMessage("كود الدكتور مطلوب")
                   .When(x => x.Role == UserRoles.InternDoctor);
        }
    }
}
