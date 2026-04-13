
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events
{
    public record BookingPostponedEvent(int BookingId, int SessionId, string Reason) : IDomainEvent;
}
