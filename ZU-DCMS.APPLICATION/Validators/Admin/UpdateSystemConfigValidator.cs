using FluentValidation;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    public class UpdateSystemConfigValidator : AbstractValidator<UpdateConfigCommand>
    {
        // __ This validator ensures that the data provided for updating a system configuration is valid. __ //
        private static readonly string[] ValidKeys =
        [
            ConfigKeys.MaxDailyPatients,
            ConfigKeys.MaxNewPerSession,
            ConfigKeys.MaxFollowUpPerSession,
            ConfigKeys.DiagnosisFee,
            ConfigKeys.SessionTimes,
            ConfigKeys.WorkingDays
        ];

        // __ Constructor to define validation rules. __ //
        public UpdateSystemConfigValidator()
        {
            RuleFor(x => x.Key)
                   .NotEmpty()
                   .WithMessage("Key is required")
                   .Must(key => ValidKeys.Contains(key))
                   .WithMessage("Invalid Key");

            RuleFor(x => x.Value)
                   .NotEmpty()
                   .WithMessage("Value is required")
                   .Must((dto, value) => BeValidValue(dto.Key, value))
                   .WithMessage("Invalid value for this setting");
        }

        // __ Helper method to validate the value based on the key. __ //
        private bool BeValidValue(string key, string value)
        {
            return key switch
            {
                ConfigKeys.MaxDailyPatients or
                ConfigKeys.MaxNewPerSession or
                ConfigKeys.MaxFollowUpPerSession => int.TryParse(value, out var n) && n > 0,

                ConfigKeys.DiagnosisFee =>  decimal.TryParse(value, out var d) && d > 0,

                ConfigKeys.SessionTimes => BeValidSessionTimes(value),

                ConfigKeys.WorkingDays => BeValidWorkingDays(value),

                _ => false
            };
        }

        // __ Validates that the session times are in the correct format and represent valid times. __ //
        private bool BeValidSessionTimes(string value)
        {
            // __ Format: "09:00,11:00,13:00,15:00" __ //
            var times = value.Split(',');
            return times.Length > 0 && times.All(t => TimeSpan.TryParse(t.Trim(), out _));
        }

        private bool BeValidWorkingDays(string value)
        {
            // __ Format: "0,1,2,3,4,6" (0=Sunday ... 6=Saturday) __ //
            var days = value.Split(',');
            return days.Length > 0 && days.All(d => int.TryParse(d.Trim(), out var n) && n >= 0 && n <= 6);
        }
    }
}
