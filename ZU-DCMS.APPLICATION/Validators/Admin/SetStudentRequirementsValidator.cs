using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class SetStudentRequirementsValidator : AbstractValidator<SetStudentRequirementsCommand>
    {
        // __ Validator for setting student requirements via SetStudentRequirementsCommand __ //
        public SetStudentRequirementsValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0)
                .WithMessage("الطالب مطلوب");

            RuleFor(x => x.TermId)
                .GreaterThan(0)
                .WithMessage("الفصل الدراسي مطلوب");

            RuleFor(x => x.AdminId)
                .NotEmpty()
                .WithMessage("معرف الموظف مطلوب");

            RuleFor(x => x.Requirements)
                .NotEmpty()
                .WithMessage("قائمة المتطلبات مطلوبة");

            RuleForEach(x => x.Requirements).ChildRules(req =>
            {
                req.RuleFor(x => x.ClinicId)
                       .GreaterThan(0)
                       .WithMessage("العيادة مطلوبة");

                req.RuleFor(x => x.RequiredCount)
                       .GreaterThan(0)
                       .WithMessage("عدد الحالات المطلوبة لازم يكون أكبر من 0");
            });
        }
    }
}
