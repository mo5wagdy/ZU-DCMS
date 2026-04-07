using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class CreateTermValidator : AbstractValidator<CreateTermDto>
    {
        // __ This validator ensures that the data provided for creating a new term is valid. __ //
        public CreateTermValidator()
        {
            RuleFor(x => x.Name)
                   .NotEmpty()
                   .WithMessage("اسم الترم مطلوب")
                   .MaximumLength(100)
                   .WithMessage("الاسم لازم يكون أقل من 100 حرف");

            RuleFor(x => x.StartDate)
                   .NotEmpty()
                   .WithMessage("تاريخ البداية مطلوب")
                   .GreaterThan(DateTime.Today)
                   .WithMessage("تاريخ البداية لازم يكون في المستقبل");

            RuleFor(x => x.EndDate)
                   .NotEmpty()
                   .WithMessage("تاريخ النهاية مطلوب")
                   .GreaterThan(x => x.StartDate)
                   .WithMessage("تاريخ النهاية لازم يكون بعد تاريخ البداية");

            RuleFor(x => x.RequiredCasesCount)
                   .GreaterThan(0)
                   .WithMessage("عدد الحالات المطلوبة لازم يكون أكبر من 0");
        }
    }
}
