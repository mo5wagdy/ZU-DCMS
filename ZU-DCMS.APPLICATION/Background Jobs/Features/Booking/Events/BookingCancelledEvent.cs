
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events
{
    public record BookingCancelledEvent(int BookingId, int SessionId) : IDomainEvent;
}
