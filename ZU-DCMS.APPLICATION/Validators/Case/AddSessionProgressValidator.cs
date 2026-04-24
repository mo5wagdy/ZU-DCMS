using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress;

namespace ZU_DCMS.APPLICATION.Validators.Case
{
    public class AddSessionProgressValidator : AbstractValidator<AddSessionProgressCommand>
    {
        // __ Validator for adding session progress via AddSessionProgressCommand __ //
        public AddSessionProgressValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0)
                .WithMessage("الطالب مطلوب");

            RuleFor(x => x.TermId)
                .GreaterThan(0)
                .WithMessage("الفصل الدراسي مطلوب");

            RuleFor(x => x.Dto.CaseId)
                   .GreaterThan(0)
                   .WithMessage("الحالة مطلوبة");

            RuleFor(x => x.Dto.ClinicId)
                   .GreaterThan(0)
                   .WithMessage("العيادة مطلوبة");

            RuleFor(x => x.Dto.SessionDate)
                   .NotEmpty().WithMessage("تاريخ الجلسة مطلوب")
                   .LessThanOrEqualTo(DateTime.UtcNow)
                   .WithMessage("التاريخ لازم يكون اليوم أو في الماضي");

            RuleFor(x => x.Dto.ProcedureDone)
                   .NotEmpty().WithMessage("الإجراء اللي تم مطلوب");

            RuleFor(x => x.Dto.NextVisitPlan)
                   .MaximumLength(500)
                   .WithMessage("الخطة القادمة لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.NextVisitPlan));
        }
    }
}
