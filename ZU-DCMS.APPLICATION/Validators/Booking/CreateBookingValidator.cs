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
                .WithMessage("Invalid patient ID");

            RuleFor(x => x.Dto.BookingType)
                   .IsInEnum()
                   .WithMessage("Invalid booking type");

            RuleFor(x => x.Dto.PreferredDate)
                   .NotEmpty().WithMessage("Date is required")
                   .Must(BeWorkingDay)
                   .WithMessage("Friday is a holiday - choose another day")
                   .Must(NotBeInThePast)
                   .WithMessage("Date must be in the future");

            RuleFor(x => x.Dto.PreferredTimeSlot)
                   .NotEmpty().WithMessage("Session time is required")
                   .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                   .WithMessage("الوقت الAvailable: من 09:00 إلى 11:00 أو من 11:00 إلى 13:00 أو من 13:00 إلى 15:00");

            RuleFor(x => x.Dto.PreliminaryComplaint)
                   .MaximumLength(500)
                   .WithMessage("Complaint must be less than 500 characters")
                   .When(x => !string.IsNullOrEmpty(x.Dto.PreliminaryComplaint));
        }

        // __ Custom validation methods __ //
        private bool BeWorkingDay(DateTime date) => date.DayOfWeek != DayOfWeek.Friday;
        private bool NotBeInThePast(DateTime date) => date.Date >= DateTime.Today;
    }
}
