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
                .WithMessage("Student is required");

            RuleFor(x => x.TermId)
                .GreaterThan(0)
                .WithMessage("Term is required");

            RuleFor(x => x.AdminId)
                .NotEmpty()
                .WithMessage("Employee ID is required");

            RuleFor(x => x.Requirements)
                .NotNull()
                .WithMessage("Requirements list is required");

            RuleForEach(x => x.Requirements).ChildRules(req =>
            {
                req.RuleFor(x => x.ClinicId)
                       .GreaterThan(0)
                       .WithMessage("Clinic is required");

                req.RuleFor(x => x.RequiredCount)
                       .GreaterThanOrEqualTo(0)
                       .WithMessage("Required cases must be 0 or greater");
            });
        }
    }
}
