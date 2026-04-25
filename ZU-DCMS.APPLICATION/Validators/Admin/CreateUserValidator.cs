using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateUser;
using ZU_DCMS.Domain.UserRoles;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    // __ This validator ensures that the data provided for creating a new user is valid. __ //
    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.dto.Username)
                   .NotEmpty()
                   .WithMessage("اسم المستخدم مطلوب")
                   .MinimumLength(3)
                   .WithMessage("لازم 3 حروف على الأقل")
                   .MaximumLength(50)
                   .WithMessage("لازم أقل من 50 حرف")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("يقبل حروف وأرقام و . _ - فقط");

            RuleFor(x => x.dto.FullName)
                   .NotEmpty()
                   .WithMessage("الاسم مطلوب")
                   .MaximumLength(100)
                   .WithMessage("لازم أقل من 100 حرف");

            RuleFor(x => x.dto.Email)
                   .NotEmpty()
                   .WithMessage("الإيميل مطلوب")
                   .EmailAddress()
                   .WithMessage("الإيميل غير صحيح");

            RuleFor(x => x.dto.Password)
                   .NotEmpty()
                   .WithMessage("كلمة المرور مطلوبه")
                   .Length(8);

            RuleFor(x => x.dto.Role)
                   .NotEmpty()
                   .WithMessage("الدور مطلوب")
                   .Must(BeValidStaffRole)
                   .WithMessage("الدور غير صحيح");

            RuleFor(x => x.dto.AcademicYear)
                   .InclusiveBetween(1, 5)
                   .WithMessage("السنة الدراسية لازم تكون بين 1 و 5")
                   .When(x => x.dto.Role == UserRoles.Student);

        }

        // __ Helper method to validate staff roles. __ //
        private bool BeValidStaffRole(string role)
            => role is UserRoles.Student
                    or UserRoles.InternDoctor
                    or UserRoles.Admin
                    or UserRoles.Dean
                    or UserRoles.ViceDean
                    or UserRoles.Professor
                    or UserRoles.TeachingAssistant;
    }
}
