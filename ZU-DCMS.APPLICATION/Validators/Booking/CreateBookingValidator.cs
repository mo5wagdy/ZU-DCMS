using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Validators.Booking
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.BookingType)
                   .NotEmpty().WithMessage("نوع الحجز مطلوب")
                   .Must(x => new[] { "New", "FollowUp" }.Contains(x))
                   .WithMessage("نوع الحجز غير صحيح");

            RuleFor(x => x.SessionId)
                   .GreaterThan(0).WithMessage("السكشن مطلوب");

            RuleFor(x => x.PreliminaryComplaint)
                   .MaximumLength(500)
                   .WithMessage("الشكوى لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.PreliminaryComplaint));
        }
    }
}
