using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.RegisterPatient;
using ZU_DCMS.APPLICATION.Validators.Shared;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class RegisterPatientValidator : AbstractValidator<RegisterPatientCommand>
    {
        // __ Constructor to define validation rules for registering a patient __ //س
        public RegisterPatientValidator()
        {

            RuleFor(x => x.Dto.FullName)
                   .NotEmpty().WithMessage("Name is required")
                   .MinimumLength(3).WithMessage("Must be at least 3 characters")
                   .MaximumLength(100).WithMessage("Must be less than 100 characters");

            RuleFor(x => x.Dto.PhoneNumber)
                   .NotEmpty().WithMessage("Phone number is required")
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("Invalid phone number");

            RuleFor(x => x.Dto.Email)
                   .EmailAddress().WithMessage("Invalid email")
                   .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email), ApplyConditionTo.CurrentValidator);

            RuleFor(x => x.Dto.IdentityType)
                   .IsInEnum()
                   .WithMessage("Invalid ID type, please select a suitable ID type");

            RuleFor(x => x.Dto.IdentityNumber)
                   .NotEmpty().WithMessage("National ID is required")
                   .Must((x, number) => BeValidIdentityNumber(number, x.Dto.IdentityType))
                   .WithMessage("Invalid National ID");

            RuleFor(x => x.Dto.DateOfBirth)
                   .NotEmpty().WithMessage("Date of birth is required")
                   .Must(BeValidAge)
                   .WithMessage("Age must be between 5 and 120 years");

            RuleFor(x => x.Dto.ParentName)
                   .NotEmpty().WithMessage("Guardian name is required for children aged 5 to 17")
                   .When(x => x.Dto.IsChild);

            RuleFor(x => x.Dto.Gender)
                   .IsInEnum()
                   .WithMessage("Invalid gender");

            RuleFor(x => x.Dto.ChronicConditions)
                   .IsInEnum()
                   .WithMessage("Invalid chronic diseases");

            RuleFor(x => x.Dto.OtherConditions)
                   .MaximumLength(500)
                   .WithMessage("Must be less than 500 characters")
                   .When(x => !string.IsNullOrEmpty(x.Dto.OtherConditions));
            
            RuleFor(x => x.Dto.Address)
                   .NotEmpty().WithMessage("Address is required")
                   .ValidAddress()
                   .When(x => !string.IsNullOrEmpty(x.Dto.Address));
        }

        // _____________ Custom Validation Methods _____________ //


        // __ Validates the identity number based on the selected identity Type __ //
        private bool BeValidIdentityNumber(string number, IdentityType type)
        {
            return type switch
            {
                IdentityType.NationalId => BeValidEgyptianNationalId(number),

                IdentityType.Passport =>
                    number.Length >= 6 &&
                    number.Length <= 20 &&
                    number.All(c => char.IsLetterOrDigit(c)),

                IdentityType.ResidencePermit =>
                    number.Length >= 6 &&
                    number.Length <= 20 &&
                    number.All(c => char.IsLetterOrDigit(c)),

                _ => false
            };
        }

        // __ Validates the Egyptian National ID format and content __ //
        private bool BeValidEgyptianNationalId(string number)
        {
            if (number.Length != 14) return false;
            if (!number.All(char.IsDigit)) return false;
            if (number[0] != '2' && number[0] != '3') return false;

            var month = int.Parse(number.Substring(3, 2));
            var day = int.Parse(number.Substring(5, 2));

            if (month < 1 || month > 12) return false;
            if (day < 1 || day > 31) return false;

            try
            {
                var century = number[0] == '2' ? 1900 : 2000;
                var year = int.Parse(number.Substring(1, 2));
                var date = new DateTime(century + year, month, day);
                if (date > DateTime.Today && (!BeValidAge(date))) return false;
            }
            catch { return false; }

            var govCode = number.Substring(7, 2);
            var validGovCodes = new HashSet<string>
            {
            "01","02","03","04","11","12","13","14",
            "15","16","17","18","19","21","22","23",
            "24","25","26","27","28","29","31","32",
            "33","34","35","88"
            };
            if (!validGovCodes.Contains(govCode)) return false;

            var serial = int.Parse(number.Substring(9, 3));
            if (serial == 0) return false;

            return true;
        }

        // __ Validates that the age calculated from the date of birth is between 1 and 120 years __ //
        private bool BeValidAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age >= 5 && age <= 120;
        }
    }

}
