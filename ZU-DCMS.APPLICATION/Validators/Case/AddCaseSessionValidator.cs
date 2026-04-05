using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Validators.Case
{
    public class AddCaseSessionValidator : AbstractValidator<AddCaseSessionDto>
    {
        public AddCaseSessionValidator()
        {
            RuleFor(x => x.CaseAssignmentId)
                   .GreaterThan(0).WithMessage("الحالة مطلوبة");

            RuleFor(x => x.ProceduresDone)
                   .NotEmpty().WithMessage("الإجراءات المنفذة مطلوبة")
                   .MaximumLength(1000).WithMessage("الإجراءات لازم تكون أقل من 1000 حرف");

            RuleFor(x => x.Notes)
                   .MaximumLength(500).WithMessage("الملاحظات لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
