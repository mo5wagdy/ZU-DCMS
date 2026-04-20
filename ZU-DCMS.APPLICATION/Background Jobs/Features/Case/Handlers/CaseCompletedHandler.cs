
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Events;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.SignalR;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Cache;
using ZU_DCMS.APPLICATION.Contracts.SignalR;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Handlers
{
    public class CaseCompletedHandler
    {
        private readonly INotificationService _notification;
        private readonly ISignalRService _signalR;
        private readonly ICacheService _cache;

        public CaseCompletedHandler
        (
            INotificationService notification,
            ISignalRService signalR,
            ICacheService cache
        )
        {
            _notification = notification;
            _signalR = signalR;
            _cache = cache; 
        }

        public async Task HandleAsync(CaseCompletedEvent e)
        {
            await _cache.RemoveAsync(CacheKeys.StudentProgress(e.studentId));

            await _cache.RemoveAsync(CacheKeys.AvailableStudents(e.clinicId));

            await _notification.SendCaseCompletedAsync(e.assignmentId);

            await _signalR.SendDashboardUpdateAsync(SignalREvents.CaseCompleted, new { e.assignmentId });
        }

    }
}
