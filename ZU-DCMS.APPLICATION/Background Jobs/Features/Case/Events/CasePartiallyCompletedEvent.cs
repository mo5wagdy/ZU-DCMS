
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Events
{
    public record CasePartiallyCompletedEvent(int studentId, int assignmentId, int clinicId) : IDomainEvent;
}
