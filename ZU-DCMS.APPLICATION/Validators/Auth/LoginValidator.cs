using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.Login;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        // __ Validator for patient login via LoginCommand __ //
        public LoginValidator()
        {
            RuleFor(x => x.Dto.PhoneNumber)
                   .NotEmpty().WithMessage("Phone number is required")
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("Invalid phone number");

            RuleFor(x => x.Dto.IdentityNumber)
                   .NotEmpty()
                   .WithMessage("National ID is required")
                   .When(x => x.Dto != null);
        }
    }
}
