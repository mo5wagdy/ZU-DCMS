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
                   .NotEmpty()
                   .WithMessage("نوع التشخيص غير صحيح");

            RuleFor(x => x.Dto.Complaint)
                   .NotEmpty()
                   .WithMessage("التشخيص مطلوب")
                   .MaximumLength(1000)
                   .WithMessage("التشخيص لازم يكون أقل من 1000 حرف");

            RuleFor(x => x.Dto.Notes)
                   .MaximumLength(1000)
                   .WithMessage("الملاحظات لازم تكون أقل من 1000 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Notes));
        }
    }
}
