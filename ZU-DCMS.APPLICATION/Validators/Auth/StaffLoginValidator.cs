using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class StaffLoginValidator : AbstractValidator<StaffLoginDto>
    {
        public StaffLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("الإيميل مطلوب");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("كلمة المرور مطلوبة");
        }
    }
}
