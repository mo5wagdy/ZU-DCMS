using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Validators.Patient
{
    public class UpdatePatientValidator : AbstractValidator<UpdatePatientDto>
    {
        // __ Validator for updating patient data __ //
        public UpdatePatientValidator()
        {
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
                   .Must(BeValidAddress)
                   .WithMessage("العنوان لازم يكون: الدولة,المحافظة")
                   .When(x => !string.IsNullOrEmpty(x.Address));
        }

        // __ Custom validation method for address format __ //
        private bool BeValidAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return true;
            var parts = address.Split(',');
            return parts.Length == 2 && parts.All(p => !string.IsNullOrWhiteSpace(p.Trim()));
        }
    }
}
