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
                .WithMessage("الإيميل مطلوب")
                .EmailAddress()
                .WithMessage("الإيميل غير صالح");

            RuleFor(x => x.Dto.Password)
                .NotEmpty()
                .WithMessage("كلمة المرور مطلوبة");
        }
    }
}
