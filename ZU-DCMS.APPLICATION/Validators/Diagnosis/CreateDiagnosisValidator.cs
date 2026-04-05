using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Validators.Diagnosis
{
    public class CreateDiagnosisValidator : AbstractValidator<CreateDiagnosisDto>
    {
        public CreateDiagnosisValidator()
        {
            RuleFor(x => x.BookingId)
                   .GreaterThan(0).WithMessage("الحجز مطلوب");

            RuleFor(x => x.ClinicId)
                   .GreaterThan(0).WithMessage("العيادة مطلوبة");

            RuleFor(x => x.Complaint)
                   .NotEmpty().WithMessage("الشكوى مطلوبة")
                   .MaximumLength(500).WithMessage("الشكوى لازم تكون أقل من 500 حرف");

            RuleFor(x => x.Diagnosis)
                   .NotEmpty().WithMessage("التشخيص مطلوب")
                   .MaximumLength(500).WithMessage("التشخيص لازم يكون أقل من 500 حرف");

            RuleFor(x => x.Notes)
                   .MaximumLength(1000).WithMessage("الملاحظات لازم تكون أقل من 1000 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
