using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent;

namespace ZU_DCMS.APPLICATION.Validators.Diagnosis
{
    public class AssignStudentValidator : AbstractValidator<AssignStudentCommand>
    {
        // __ Validator for assigning a student via AssignStudentCommand __ //
        public AssignStudentValidator()
        {
            RuleFor(x => x.InternDoctorId)
                .NotEmpty()
                .WithMessage("معرف الطبيب مطلوب");

            RuleFor(x => x.Dto.DiagnosisRecordId)
                   .GreaterThan(0)
                   .WithMessage("التشخيص مطلوب");

            RuleFor(x => x.Dto.StudentId)
                   .GreaterThan(0)
                   .WithMessage("الطالب مطلوب");

            RuleFor(x => x.Dto.Notes)
                   .MaximumLength(500)
                   .WithMessage("الملاحظات لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Notes));
        }
    }
}
