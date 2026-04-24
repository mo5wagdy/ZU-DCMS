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
                .WithMessage("معرف الطبيب مطلوب");

            RuleFor(x => x.Dto.DiagnosisTypeId)
                   .IsInEnum()
                   .WithMessage("نوع التشخيص غير صحيح");

            RuleFor(x => x.Dto.InitialDiagnosis)
                   .NotEmpty()
                   .WithMessage("التشخيص المبدئي مطلوب")
                   .MaximumLength(1000)
                   .WithMessage("التشخيص المبدئي لازم يكون أقل من 1000 حرف");

            RuleFor(x => x.Dto.FinalDiagnosis)
                   .MaximumLength(1000)
                   .WithMessage("التشخيص النهائي لازم يكون أقل من 1000 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.FinalDiagnosis));

            RuleFor(x => x.Dto.SpecialtyId)
                   .GreaterThan(0)
                   .WithMessage("التخصص مطلوب");

            RuleFor(x => x.Dto.Notes)
                   .MaximumLength(1000)
                   .WithMessage("الملاحظات لازم تكون أقل من 1000 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Notes));
        }
    }
}
