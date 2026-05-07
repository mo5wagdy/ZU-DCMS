using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient;

namespace ZU_DCMS.APPLICATION.Validators.Diagnosis
{
    public class DiagnosePatientValidator : AbstractValidator<DiagnosePatientCommand>
    {
        // __ Validator for diagnosing a patient via DiagnosePatientCommand __ //
        public DiagnosePatientValidator()
        {
            RuleFor(x => x.InternDoctorId)
                .NotEmpty()
                .WithMessage("Doctor ID is required");

            RuleFor(x => x.Dto.DiagnosisTypeId)
                   .NotEmpty()
                   .WithMessage("Invalid diagnosis type");

            RuleFor(x => x.Dto.Complaint)
                   .NotEmpty()
                   .WithMessage("Diagnosis is required")
                   .MaximumLength(1000)
                   .WithMessage("Diagnosis must be less than 1000 characters");

            RuleFor(x => x.Dto.Notes)
                   .MaximumLength(1000)
                   .WithMessage("Notes must be less than 1000 characters")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Notes));
        }
    }
}
