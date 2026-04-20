
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.SignalR;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Cache;
using ZU_DCMS.APPLICATION.Contracts.SignalR;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Handlers
{
    public class BookingCancelledHandler : IEventHandler<BookingCancelledEvent>
    {
        private readonly INotificationService _notification;
        private readonly ISignalRService _signalR;
        private readonly ICacheService _cache;

        public BookingCancelledHandler(
            INotificationService notification,
            ISignalRService signalR,
            ICacheService cache)
        {
            _notification = notification;
            _signalR = signalR;
            _cache = cache;
        }

        public async Task HandleAsync(BookingCancelledEvent e)
        {
            await _cache.RemoveAsync(CacheKeys.SessionStatus(e.SessionId));

            await _notification.SendBookingCancelledAsync(e.BookingId);

            await _signalR.SendDashboardUpdateAsync(
                SignalREvents.BookingCancelled,
                new { bookingId = e.BookingId, sessionId = e.SessionId });
        }
    }
}
