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
                .WithMessage("Student is required");

            RuleFor(x => x.TermId)
                .GreaterThan(0)
                .WithMessage("Term is required");

            RuleFor(x => x.Dto.CaseAssignmentId)
                   .GreaterThan(0)
                   .WithMessage("Status is required");

            RuleFor(x => x.Dto.ProcedureIds)
                   .NotEmpty()
                   .WithMessage("Performed procedure is required");
        }
    }
}
