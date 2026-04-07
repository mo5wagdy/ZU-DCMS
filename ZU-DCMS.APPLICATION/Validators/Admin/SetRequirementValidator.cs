using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.APPLICATION.Validators.Admin
{
    // __ This validator ensures that the data provided for setting the requirement of a clinic is valid. __ //
    public class SetRequirementValidator : AbstractValidator<SetRequirementDto>
    {
        public SetRequirementValidator()
        {
            RuleFor(x => x.ClinicId)
                   .GreaterThan(0)
                   .WithMessage("العيادة مطلوبة");

            RuleFor(x => x.RequiredCount)
                   .GreaterThan(0)
                   .WithMessage("عدد الحالات المطلوبة لازم يكون أكبر من 0");
        }
    }
}
