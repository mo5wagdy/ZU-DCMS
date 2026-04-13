
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Events
{
    public record PaymentCompletedEvent(int PaymentId, int BookingId) : IDomainEvent;
}
