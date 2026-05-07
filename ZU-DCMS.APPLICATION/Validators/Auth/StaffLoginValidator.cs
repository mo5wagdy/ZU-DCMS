using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.StaffLogin;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class StaffLoginValidator : AbstractValidator<StaffLoginCommand>
    {
        // __ Validator for staff login via StaffLoginCommand __ //
        public StaffLoginValidator()
        {
            RuleFor(x => x.Dto.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email");

            RuleFor(x => x.Dto.Password)
                .NotEmpty()
                .WithMessage("Password is required");
        }
    }
}
