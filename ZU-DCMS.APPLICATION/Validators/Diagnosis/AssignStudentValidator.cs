using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Validators.Diagnosis
{
    public class AssignStudentValidator : AbstractValidator<AssignStudentDto>
    {
        // __ This validator ensures that the AssignStudentDto object adheres to the specified rules. __ //
        public AssignStudentValidator()
        {
            RuleFor(x => x.DiagnosisRecordId)
                   .GreaterThan(0)
                   .WithMessage("التشخيص مطلوب");

            RuleFor(x => x.StudentId)
                   .GreaterThan(0)
                   .WithMessage("الطالب مطلوب");

            RuleFor(x => x.Notes)
                   .MaximumLength(500)
                   .WithMessage("الملاحظات لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
