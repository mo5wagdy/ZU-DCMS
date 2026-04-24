using FluentValidation;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.CreateBooking;

namespace ZU_DCMS.APPLICATION.Validators.Booking
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        // __ Constructor to set up validation rules for CreateBookingCommand __ //
        public CreateBookingValidator()
        {
            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .WithMessage("معرف المريض غير صحيح");

            RuleFor(x => x.Dto.BookingType)
                   .IsInEnum()
                   .WithMessage("نوع الحجز غير صحيح");

            RuleFor(x => x.Dto.PreferredDate)
                   .NotEmpty().WithMessage("التاريخ مطلوب")
                   .Must(BeWorkingDay)
                   .WithMessage("الجمعة إجازة - اختار يوم تاني")
                   .Must(NotBeInThePast)
                   .WithMessage("التاريخ لازم يكون في المستقبل");

            RuleFor(x => x.Dto.PreferredTimeSlot)
                   .NotEmpty().WithMessage("وقت السكشن مطلوب")
                   .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                   .WithMessage("الوقت المتاح: من 09:00 إلى 11:00 أو من 11:00 إلى 13:00 أو من 13:00 إلى 15:00");

            RuleFor(x => x.Dto.PreliminaryComplaint)
                   .MaximumLength(500)
                   .WithMessage("الشكوى لازم تكون أقل من 500 حرف")
                   .When(x => !string.IsNullOrEmpty(x.Dto.PreliminaryComplaint));
        }

        // __ Custom validation methods __ //
        private bool BeWorkingDay(DateTime date) => date.DayOfWeek != DayOfWeek.Friday;
        private bool NotBeInThePast(DateTime date) => date.Date >= DateTime.Today;
    }
}
