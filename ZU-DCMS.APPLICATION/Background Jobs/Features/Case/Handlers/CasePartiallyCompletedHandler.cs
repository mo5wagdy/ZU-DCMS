
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Handlers
{
    public class CasePartiallyCompletedHandler
    {
        private readonly INotificationService _notification;
        private readonly ISignalRService _signalR;
        private readonly ICacheService _cache;

        public CasePartiallyCompletedHandler
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

        public async Task HandleAsync(CasePartiallyCompletedEvent e)
        {
            await _cache.RemoveAsync(CacheKeys.StudentProgress(e.studentId));

            await _cache.RemoveAsync(CacheKeys.AvailableStudents(e.clinicId));

            await _notification.SendCasePartiallyCompletedAsync(e.assignmentId);

            await _signalR.SendDashboardUpdateAsync(SignalREvents.CaseHasFollowUp, new { e.assignmentId });
        }
    }
}
