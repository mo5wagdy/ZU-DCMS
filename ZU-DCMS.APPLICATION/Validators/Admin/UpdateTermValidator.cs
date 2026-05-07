using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class UpdateTermValidator : AbstractValidator<UpdateTermCommand>
    {
        // __ Validator for updating an existing term via UpdateTermCommand __ //
        public UpdateTermValidator()
        {
            RuleFor(x => x.TermId)
                .GreaterThan(0)
                .WithMessage("Invalid term ID");

            RuleFor(x => x.AdminId)
                .NotEmpty()
                .WithMessage("Manager ID is required");

            RuleFor(x => x.Dto.Name)
                   .MaximumLength(100)
                   .WithMessage("Name must be less than 100 characters")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Name));

            RuleFor(x => x.Dto.StartDate)
                   .GreaterThan(DateTime.Today)
                   .WithMessage("Start date must be in the future")
                   .When(x => x.Dto.StartDate.HasValue);

            RuleFor(x => x.Dto.EndDate)
                   .GreaterThan(x => x.Dto.StartDate ?? DateTime.Today)
                   .WithMessage("End date must be after start date")
                   .When(x => x.Dto.EndDate.HasValue);

            RuleFor(x => x.Dto.RequiredCasesCount)
                   .GreaterThan(0)
                   .WithMessage("Number of cases must be greater than 0")
                   .When(x => x.Dto.RequiredCasesCount.HasValue);
        }
    }
}
