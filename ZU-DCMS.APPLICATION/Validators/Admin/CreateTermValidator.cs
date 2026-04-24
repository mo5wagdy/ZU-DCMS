using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class CreateTermValidator : AbstractValidator<CreateTermCommand>
    {
        // __ This validator ensures that the data provided for creating a new term is valid. __ //
        public CreateTermValidator()
        {
            RuleFor(x => x.Dto.Name)
                   .NotEmpty()
                   .WithMessage("اسم الترم مطلوب")
                   .MaximumLength(100)
                   .WithMessage("الاسم لازم يكون أقل من 100 حرف");

            RuleFor(x => x.Dto.StartDate)
                   .NotEmpty()
                   .WithMessage("تاريخ البداية مطلوب")
                   .GreaterThan(DateTime.Today)
                   .WithMessage("تاريخ البداية لازم يكون في المستقبل");

            RuleFor(x => x.Dto.EndDate)
                   .NotEmpty()
                   .WithMessage("تاريخ النهاية مطلوب")
                   .GreaterThan(x => x.Dto.StartDate)
                   .WithMessage("تاريخ النهاية لازم يكون بعد تاريخ البداية");

            RuleFor(x => x.Dto.RequiredCasesCount)
                   .GreaterThan(0)
                   .WithMessage("عدد الحالات المطلوبة لازم يكون أكبر من 0");
        }
    }
}