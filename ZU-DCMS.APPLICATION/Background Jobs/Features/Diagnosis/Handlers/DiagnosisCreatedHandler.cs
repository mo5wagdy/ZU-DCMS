
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Handlers
{
    public class DiagnosisCreatedHandler : IEventHandler<DiagnosisCreatedEvent>
    {
        private readonly INotificationService _notification;
        private readonly ISignalRService _signalR;
        public DiagnosisCreatedHandler(INotificationService notification, ISignalRService signalR)
        {
            _notification = notification;
            _signalR = signalR;
        }

        public async Task HandleAsync(DiagnosisCreatedEvent e)
        {
            await _notification.SendDiagnosisCreatedAsync(e.DiagnosisId);
            await _signalR.SendDashboardUpdateAsync(SignalREvents.DiagnosisCreated, new { diagnosisId = e.DiagnosisId});
        }
    }
}
