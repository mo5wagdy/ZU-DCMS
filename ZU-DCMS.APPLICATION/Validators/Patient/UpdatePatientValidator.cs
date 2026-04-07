using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.APPLICATION.Validators.Shared;

namespace ZU_DCMS.APPLICATION.Validators.Patient
{
    public class UpdatePatientValidator : AbstractValidator<UpdatePatientDto>
    {
        // __ Validator for updating patient data __ //
        public UpdatePatientValidator()
        {
            RuleFor(x => x.Username)
                   .MinimumLength(3).WithMessage("لازم 3 حروف على الأقل")
                   .MaximumLength(50).WithMessage("لازم أقل من 50 حرف")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("يقبل حروف وأرقام و . _ - فقط")
                   .When(x => !string.IsNullOrEmpty(x.Username));

            RuleFor(x => x.PhoneNumber)
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("رقم التليفون غير صحيح")
                   .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Email)
                   .EmailAddress().WithMessage("الإيميل غير صحيح")
                   .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.ChronicConditions)
                   .IsInEnum().WithMessage("الأمراض المزمنة غير صحيحة")
                   .When(x => x.ChronicConditions.HasValue);

            RuleFor(x => x.OtherConditions)
                   .MaximumLength(500)
                   .WithMessage("لازم يكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.OtherConditions));

            RuleFor(x => x.Address)
                   .ValidAddress()
                   .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}
