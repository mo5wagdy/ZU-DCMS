
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Events
{
    public record CaseCompletedEvent(int studentId, int assignmentId, int clinicId ) : IDomainEvent;
}
