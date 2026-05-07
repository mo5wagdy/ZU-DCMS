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
            RuleFor(x => x.Dto.Username)
                   .NotEmpty()
                   .WithMessage("Username is required")
                   .MinimumLength(3)
                   .WithMessage("Must be at least 3 characters")
                   .MaximumLength(50)
                   .WithMessage("Must be less than 50 characters")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("Accepts only letters, numbers, and . _ -");

            RuleFor(x => x.Dto.FullName)
                   .NotEmpty()
                   .WithMessage("Name is required")
                   .MaximumLength(100)
                   .WithMessage("Must be less than 100 characters");

            RuleFor(x => x.Dto.Email)
                   .NotEmpty()
                   .WithMessage("Email is required")
                   .EmailAddress()
                   .WithMessage("Invalid email");

            RuleFor(x => x.Dto.Password)
                   .NotEmpty()
                   .WithMessage("Password is required")
                   .MinimumLength(8)
                   .WithMessage("Must be at least 8 characters");

            RuleFor(x => x.Dto.Role)
                   .NotEmpty()
                   .WithMessage("Role is required")
                   .Must(BeValidStaffRole)
                   .WithMessage("Invalid role");

            RuleFor(x => x.Dto.AcademicYear)
                   .InclusiveBetween(1, 5)
                   .WithMessage("Academic year must be between 1 and 5")
                   .When(x => x.Dto.Role == UserRoles.Student);

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
