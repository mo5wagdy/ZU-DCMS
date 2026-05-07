using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class CreateTermValidator : AbstractValidator<CreateTermCommand>
    {
        // __ This validator ensures that the data provided for creating a new term is valid. __ //
        public CreateTermValidator()
        {
            RuleFor(x => x.Dto.Name)
                   .NotEmpty()
                   .WithMessage("Term name is required")
                   .MaximumLength(100)
                   .WithMessage("Name must be less than 100 characters");

            RuleFor(x => x.Dto.StartDate)
                   .NotEmpty()
                   .WithMessage("Start date is required")
                   .GreaterThan(DateTime.Today)
                   .WithMessage("Start date must be in the future");

            RuleFor(x => x.Dto.EndDate)
                   .NotEmpty()
                   .WithMessage("End date is required")
                   .GreaterThan(x => x.Dto.StartDate)
                   .WithMessage("End date must be after start date");

            RuleFor(x => x.Dto.RequiredCasesCount)
                   .GreaterThan(0)
                   .WithMessage("Required cases must be greater than 0");
        }
    }
}