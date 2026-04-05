using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class RegisterPatientValidator : AbstractValidator<RegisterPatientDto>
    {
        public RegisterPatientValidator()
        {
            RuleFor(x => x.Username)
                   .NotEmpty().WithMessage("اسم المستخدم مطلوب")
                   .MinimumLength(3).WithMessage("اسم المستخدم لازم يكون 3 حروف على الأقل")
                   .MaximumLength(50).WithMessage("اسم المستخدم لازم يكون أقل من 50 حرف");

            RuleFor(x => x.FullName)
                   .NotEmpty().WithMessage("الاسم رباعي مطلوب")
                   .MaximumLength(100).WithMessage("الاسم لازم يكون أقل من 100 حرف");

            RuleFor(x => x.PhoneNumber)
                   .NotEmpty().WithMessage("رقم التليفون مطلوب")
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("رقم التليفون غير صحيح");

            RuleFor(x => x.Email)
                   .EmailAddress().WithMessage("الإيميل غير صحيح")
                   .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.IdentityNumber)
                   .NotEmpty().WithMessage("رقم الهوية مطلوب")
                   .MinimumLength(6).WithMessage("رقم الهوية غير صحيح")
                   .MaximumLength(20).WithMessage("رقم الهوية غير صحيح");

            RuleFor(x => x.IdentityType)
                   .NotEmpty().WithMessage("نوع الهوية مطلوب")
                   .IsInEnum().WithMessage("نوع الهوية غير صحيح");

            RuleFor(x => x.NationalityCode)
                   .NotEmpty().WithMessage("الجنسية مطلوبة")
                   .MaximumLength(3).WithMessage("كود الجنسية غير صحيح");

            RuleFor(x => x.DateOfBirth)
                   .NotEmpty().WithMessage("تاريخ الميلاد مطلوب")
                   .Must(BeValidAge).WithMessage("السن لازم يكون بين 1 و 120 سنة");

            RuleFor(x => x.Gender)
                   .NotEmpty().WithMessage("النوع مطلوب")
                   .IsInEnum().WithMessage("النوع غير صحيح");
        }

        private bool BeValidAge(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            return age >= 1 && age <= 120;
        }
    }

}
