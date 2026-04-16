using FluentValidation;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    // __ This validator ensures that the data provided for creating a new user is valid. __ //
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                   .NotEmpty()
                   .WithMessage("اسم المستخدم مطلوب")
                   .MinimumLength(3)
                   .WithMessage("لازم 3 حروف على الأقل")
                   .MaximumLength(50)
                   .WithMessage("لازم أقل من 50 حرف")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("يقبل حروف وأرقام و . _ - فقط");

            RuleFor(x => x.FullName)
                   .NotEmpty()
                   .WithMessage("الاسم مطلوب")
                   .MaximumLength(100)
                   .WithMessage("لازم أقل من 100 حرف");

            RuleFor(x => x.Email)
                   .NotEmpty()
                   .WithMessage("الإيميل مطلوب")
                   .EmailAddress()
                   .WithMessage("الإيميل غير صحيح");

            RuleFor(x => x.Password)
                   .NotEmpty()
                   .WithMessage("كلمة المرور مطلوبه")
                   .Length(8);

            RuleFor(x => x.Role)
                   .NotEmpty()
                   .WithMessage("الدور مطلوب")
                   .Must(BeValidStaffRole)
                   .WithMessage("الدور غير صحيح");

            RuleFor(x => x.AcademicYear)
                   .InclusiveBetween(1, 5)
                   .WithMessage("السنة الدراسية لازم تكون بين 1 و 5")
                   .When(x => x.Role == UserRoles.Student);

        }

        // __ Helper method to validate staff roles. __ //
        private bool BeValidStaffRole(string role)
            => role is UserRoles.Student
                    or UserRoles.InternDoctor
                    or UserRoles.Admin;
    }
}
