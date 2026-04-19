using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.CreateBooking
{
    public record CreateBookingCommand(int PatientId, CreateBookingDto Dto) : IRequest<Result<BookingDto>>;
}
