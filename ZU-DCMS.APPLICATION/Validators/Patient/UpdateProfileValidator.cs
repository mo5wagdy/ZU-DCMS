using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Patients.Commands.UpdateProfile;
using ZU_DCMS.APPLICATION.Validators.Shared;

namespace ZU_DCMS.APPLICATION.Validators.Patient
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        // __ Validator for updating patient data via UpdateProfileCommand __ //
        public UpdateProfileValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("معرف المريض غير صحيح");

            RuleFor(x => x.Dto.Username)
                   .MinimumLength(3).WithMessage("لازم 3 حروف على الأقل")
                   .MaximumLength(50).WithMessage("لازم أقل من 50 حرف")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("يقبل حروف وأرقام و . _ - فقط")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Username));

            RuleFor(x => x.Dto.PhoneNumber)
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("رقم التليفون غير صحيح")
                   .When(x => !string.IsNullOrEmpty(x.Dto.PhoneNumber));

            RuleFor(x => x.Dto.Email)
                   .EmailAddress().WithMessage("الإيميل غير صحيح")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Email));

            RuleFor(x => x.Dto.ChronicConditions)
                   .IsInEnum().WithMessage("الأمراض المزمنة غير صحيحة")
                   .When(x => x.Dto.ChronicConditions.HasValue);

            RuleFor(x => x.Dto.OtherConditions)
                   .MaximumLength(500)
                   .WithMessage("لازم يكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.OtherConditions));

            RuleFor(x => x.Dto.Address)
                   .ValidAddress()
                   .When(x => !string.IsNullOrEmpty(x.Dto.Address));
        }
    }
}
