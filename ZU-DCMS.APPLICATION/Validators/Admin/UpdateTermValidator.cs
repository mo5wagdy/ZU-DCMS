using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class UpdateTermValidator : AbstractValidator<UpdateTermDto>
    {
        // __ This validator ensures that the data provided for updating an existing term is valid. __ //
        public UpdateTermValidator()
        {
            RuleFor(x => x.Name)
                   .MaximumLength(100)
                   .WithMessage("الاسم لازم يكون أقل من 100 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.StartDate)
                   .GreaterThan(DateTime.Today)
                   .WithMessage("تاريخ البداية لازم يكون في المستقبل")
                   .When(x => x.StartDate.HasValue);

            RuleFor(x => x.EndDate)
                   .GreaterThan(x => x.StartDate ?? DateTime.Today)
                   .WithMessage("تاريخ النهاية لازم يكون بعد تاريخ البداية")
                   .When(x => x.EndDate.HasValue);

            RuleFor(x => x.RequiredCasesCount)
                   .GreaterThan(0)
                   .WithMessage("عدد الحالات لازم يكون أكبر من 0")
                   .When(x => x.RequiredCasesCount.HasValue);
        }
    }
}
