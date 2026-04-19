using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.CancelBooking
{
    public record CancelBookingCommand(int BookingId, int PatientId) : IRequest<Result>;
}
