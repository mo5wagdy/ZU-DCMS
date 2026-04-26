using FluentValidation;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Validators.Case
{
    public class ReviewCaseValidator : AbstractValidator<ReviewCaseDto>
    {
        public ReviewCaseValidator()
        {
            RuleFor(x => x.CaseAssignmentId)
                   .GreaterThan(0)
                   .WithMessage("الحالة مطلوبة");

            RuleFor(x => x.Notes)
                   .NotEmpty()
                   .WithMessage("يجب إعطاء ملحوظات عن الحالة في حالة القبول أو الرفض")
                   .MaximumLength(500);
        }
    }
}
