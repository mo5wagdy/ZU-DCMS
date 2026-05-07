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
                .WithMessage("Invalid patient ID");

            RuleFor(x => x.Dto.Username)
                   .MinimumLength(3).WithMessage("Must be at least 3 characters")
                   .MaximumLength(50).WithMessage("Must be less than 50 characters")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("Accepts only letters, numbers, and . _ -")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Username));

            RuleFor(x => x.Dto.PhoneNumber)
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("Invalid phone number")
                   .When(x => !string.IsNullOrEmpty(x.Dto.PhoneNumber));

            RuleFor(x => x.Dto.Email)
                   .EmailAddress().WithMessage("Invalid email")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Email));

            RuleFor(x => x.Dto.ChronicConditions)
                   .IsInEnum().WithMessage("Invalid chronic diseases")
                   .When(x => x.Dto.ChronicConditions.HasValue);

            RuleFor(x => x.Dto.OtherConditions)
                   .MaximumLength(500)
                   .WithMessage("Must be less than 500 characters")
                   .When(x => !string.IsNullOrEmpty(x.Dto.OtherConditions));

            RuleFor(x => x.Dto.Address)
                   .ValidAddress()
                   .When(x => !string.IsNullOrEmpty(x.Dto.Address));
        }
    }
}
