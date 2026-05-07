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
                .WithMessage("Doctor ID is required");

            RuleFor(x => x.Dto.DiagnosisRecordId)
                   .GreaterThan(0)
                   .WithMessage("Diagnosis is required");

            RuleFor(x => x.Dto.StudentId)
                   .GreaterThan(0)
                   .WithMessage("Student is required");

            RuleFor(x => x.Dto.Notes)
                   .MaximumLength(500)
                   .WithMessage("Notes must be less than 500 characters")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Notes));
        }
    }
}
