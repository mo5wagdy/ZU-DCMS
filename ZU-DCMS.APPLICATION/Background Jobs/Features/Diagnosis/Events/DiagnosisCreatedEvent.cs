
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events
{
    // __ Event triggered when a diagnosis is created for a booking. __ //
    public record DiagnosisCreatedEvent(int DiagnosisId, int BookingId) : IDomainEvent;
}
