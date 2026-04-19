using MediatR;
using ZU_DCMS.APPLICATION.Common;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.PostponeBooking
{
    public record PostponeBookingCommand(int BookingId, string Reason, string AdminId) : IRequest<Result>; 
}
