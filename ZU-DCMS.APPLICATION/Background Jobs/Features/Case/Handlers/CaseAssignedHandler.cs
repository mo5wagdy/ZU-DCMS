
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common.SignalR;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.SignalR;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Handlers
{
    public class CaseAssignedHandler : IEventHandler<CaseAssignedEvent>
    {
        private readonly INotificationService _notification;
        private readonly ISignalRService _signalR;

        public CaseAssignedHandler(INotificationService notification, ISignalRService signalR)
        {
            _notification = notification;
            _signalR = signalR;
        }

        public async Task HandleAsync(CaseAssignedEvent e)
        {
            // Send notification to the assigned student
            await _notification.SendStudentAssignedAsync(e.CaseId);
            // Notify clients about the case assignment
            await _signalR.SendDashboardUpdateAsync(SignalREvents.CaseAssigned, new { caseId = e.CaseId, studentId = e.StudentId, clinicId = e.ClinicId });
        }
    }
}
