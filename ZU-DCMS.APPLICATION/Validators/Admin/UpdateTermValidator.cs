using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class UpdateTermValidator : AbstractValidator<UpdateTermCommand>
    {
        // __ Validator for updating an existing term via UpdateTermCommand __ //
        public UpdateTermValidator()
        {
            RuleFor(x => x.TermId)
                .GreaterThan(0)
                .WithMessage("معرف الفصل الدراسي غير صحيح");

            RuleFor(x => x.AdminId)
                .NotEmpty()
                .WithMessage("معرف المدير مطلوب");

            RuleFor(x => x.Dto.Name)
                   .MaximumLength(100)
                   .WithMessage("الاسم لازم يكون أقل من 100 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Name));

            RuleFor(x => x.Dto.StartDate)
                   .GreaterThan(DateTime.Today)
                   .WithMessage("تاريخ البداية لازم يكون في المستقبل")
                   .When(x => x.Dto.StartDate.HasValue);

            RuleFor(x => x.Dto.EndDate)
                   .GreaterThan(x => x.Dto.StartDate ?? DateTime.Today)
                   .WithMessage("تاريخ النهاية لازم يكون بعد تاريخ البداية")
                   .When(x => x.Dto.EndDate.HasValue);

            RuleFor(x => x.Dto.RequiredCasesCount)
                   .GreaterThan(0)
                   .WithMessage("عدد الحالات لازم يكون أكبر من 0")
                   .When(x => x.Dto.RequiredCasesCount.HasValue);
        }
    }
}
