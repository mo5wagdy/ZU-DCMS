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
                   .WithMessage("Status is required");

            RuleFor(x => x.Notes)
                   .NotEmpty()
                   .WithMessage("Notes must be provided in case of approval or rejection")
                   .MaximumLength(500);
        }
    }
}
