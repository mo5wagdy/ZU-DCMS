using FluentValidation;
using ZU_DCMS.APPLICATION.DTOs.Auth;
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
            RuleFor(x => x.Dto.Username)
                   .NotEmpty().WithMessage("اسم المستخدم مطلوب")
                   .MinimumLength(3).WithMessage("لازم 3 حروف على الأقل")
                   .MaximumLength(50).WithMessage("لازم أقل من 50 حرف")
                   .Matches(@"^[a-zA-Z0-9._-]+$")
                   .WithMessage("يقبل حروف وأرقام و . _ - فقط");

            RuleFor(x => x.Dto.FullName)
                   .NotEmpty().WithMessage("الاسم مطلوب")
                   .MinimumLength(3).WithMessage("لازم 3 حروف على الأقل")
                   .MaximumLength(100).WithMessage("لازم أقل من 100 حرف");

            RuleFor(x => x.Dto.PhoneNumber)
                   .NotEmpty().WithMessage("رقم التليفون مطلوب")
                   .Matches(@"^\+?[0-9]{10,15}$")
                   .WithMessage("رقم التليفون غير صحيح");

            RuleFor(x => x.Dto.Email)
                   .EmailAddress().WithMessage("الإيميل غير صحيح")
                   .When(x => !string.IsNullOrEmpty(x.Dto.Email));

            RuleFor(x => x.Dto.IdentityType)
                   .IsInEnum()
                   .WithMessage("نوع الهوية غير صحيح برجاء إختيار نوع هوية مناسب");

            RuleFor(x => x.Dto.IdentityNumber)
                   .NotEmpty().WithMessage("رقم الهوية مطلوب")
                   .Must((x, number) => BeValidIdentityNumber(number, x.Dto.IdentityType))
                   .WithMessage("رقم الهوية غير صحيح");

            RuleFor(x => x.Dto.DateOfBirth)
                   .NotEmpty().WithMessage("تاريخ الميلاد مطلوب")
                   .Must(BeValidAge)
                   .WithMessage("السن لازم يكون بين 1 و 120 سنة");

            RuleFor(x => x.Dto.Gender)
                   .IsInEnum()
                   .WithMessage("النوع غير صحيح");

            RuleFor(x => x.Dto.ChronicConditions)
                   .IsInEnum()
                   .WithMessage("الأمراض المزمنة غير صحيحة");

            RuleFor(x => x.Dto.OtherConditions)
                   .MaximumLength(500)
                   .WithMessage("لازم يكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.OtherConditions));
            
            RuleFor(x => x.Dto.Address)
                   .NotEmpty().WithMessage("العنوان مطلوب")
                   .ValidAddress()
                   .When(x => !string.IsNullOrEmpty(x.Dto.Address));
        }

        // _____________ Custom Validation Methods _____________ //


        // __ Validates the identity number based on the selected identity type __ //
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
                if (date > DateTime.Today) return false;
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
            return age >= 1 && age <= 120;
        }
    }

}
