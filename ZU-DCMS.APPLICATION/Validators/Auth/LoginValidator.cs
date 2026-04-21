using FluentValidation;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class LoginValidator : AbstractValidator<LoginPatientDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.PhoneNumber)
                   .NotEmpty().WithMessage("رقم التليفون مطلوب")
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("رقم التليفون غير صحيح");

            RuleFor(x => x.IdentityNumber)
                   .NotEmpty()
                   .WithMessage("رقم الهوية مطلوب");
        }
    }
}
