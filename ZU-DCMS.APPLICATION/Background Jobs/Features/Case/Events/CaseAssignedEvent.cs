
using ZU_DCMS.APPLICATION.Background_Jobs.Events;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Case
{
    // __ Event triggered when a case is assigned to a student in a clinic. __ //
    public record CaseAssignedEvent(int CaseId, int StudentId, int ClinicId) : IDomainEvent;
}
