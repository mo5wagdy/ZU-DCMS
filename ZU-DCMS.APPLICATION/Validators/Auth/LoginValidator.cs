using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Auth;

namespace ZU_DCMS.APPLICATION.Validators.Auth
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Username)
                   .NotEmpty().WithMessage("اسم المستخدم مطلوب");

            RuleFor(x => x.Password)
                   .NotEmpty().WithMessage("كلمة المرور مطلوبة");
        }
    }
}
