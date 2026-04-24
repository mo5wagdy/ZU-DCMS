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

            RuleFor(x => x.Dto.CaseAssignmentId)
                   .GreaterThan(0)
                   .WithMessage("الحالة مطلوبة");

            RuleFor(x => x.Dto.ProcedureIds)
                   .NotEmpty().WithMessage("الإجراء اللي تم مطلوب");
        }
    }
}
