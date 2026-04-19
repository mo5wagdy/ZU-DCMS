using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdQuery(int BookingId) : IRequest<Result<BookingDto>>;
}
