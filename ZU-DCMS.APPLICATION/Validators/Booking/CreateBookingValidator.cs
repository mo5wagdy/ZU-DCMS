using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Validators.Booking
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
    {
        // __ Define valid time slots for booking __ //
        // private static readonly string[] ValidTimeSlots = ["09:00", "11:00", "13:00", "15:00"];

        // __ Constructor to set up validation rules __ //
        public CreateBookingValidator()
        {
            RuleFor(x => x.BookingType)
                   .IsInEnum()
                   .WithMessage("نوع الحجز غير صحيح");

            RuleFor(x => x.PreferredDate)
                   .NotEmpty().WithMessage("التاريخ مطلوب")
                   .Must(BeWorkingDay)
                   .WithMessage("الجمعة إجازة - اختار يوم تاني")
                   .Must(NotBeInThePast)
                   .WithMessage("التاريخ لازم يكون في المستقبل");

            RuleFor(x => x.PreferredTimeSlot)
                   .NotEmpty().WithMessage("وقت السكشن مطلوب")
                   .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                   .WithMessage("الوقت المتاح: من 09:00 إلى 11:00 أو من 11:00 إلى 13:00 أو من 13:00 إلى 15:00");

            RuleFor(x => x.PreliminaryComplaint)
                   .MaximumLength(500)
                   .WithMessage("الشكوى لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.PreliminaryComplaint));
        }

        // __ Custom validation methods __ //

        private bool BeWorkingDay(DateTime date) => date.DayOfWeek != DayOfWeek.Friday; // => Verify that the date is not a Friday (weekend)
        private bool NotBeInThePast(DateTime date) => date.Date >= DateTime.Today; // => Ensure that the preferred date is not in the past (must be today or a future date)
        
        // private bool BeValidTimeSlot(string slot) => ValidTimeSlots.Contains(slot); // => Check if the preferred time slot is one of the predefined valid time slots
    }
}
