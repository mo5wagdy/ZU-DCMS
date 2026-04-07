using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Case;

namespace ZU_DCMS.APPLICATION.Validators.Case
{
    public class AddCaseSessionValidator : AbstractValidator<AddCaseSessionDto>
    {
        // __ This validator ensures that the data provided for adding a case session is valid. __ //
        public AddCaseSessionValidator()
        {
            RuleFor(x => x.CaseAssignmentId)
                   .GreaterThan(0)
                   .WithMessage("الحالة مطلوبة");

            RuleFor(x => x.ProcedureIds)
                   .NotEmpty()
                   .WithMessage("لازم تختار إجراء واحد على الأقل")
                   .Must(ids => ids.All(id => id > 0))
                   .WithMessage("الإجراءات غير صحيحة");

            RuleFor(x => x.Notes)
                   .MaximumLength(500)
                   .WithMessage("الملاحظات لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
