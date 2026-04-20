using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetSessionBookings
{
    public record GetSessionBookingsQuery(int SessionId, PagedRequest Request) : IRequest<Result<PagedResult<BookingDto>>>;
}
