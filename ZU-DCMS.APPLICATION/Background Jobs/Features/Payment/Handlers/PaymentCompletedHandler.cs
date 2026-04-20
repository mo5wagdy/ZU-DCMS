
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Events;
using ZU_DCMS.APPLICATION.Common.SignalR;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.SignalR;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Handlers
{
    public class PaymentCompletedHandler : IEventHandler<PaymentCompletedEvent>
    {
        private readonly ISignalRService _signalR;
        private readonly INotificationService _notification;

        public PaymentCompletedHandler(ISignalRService signalR, INotificationService notification)
        {
            _signalR = signalR;
            _notification = notification;
        }

        public async Task HandleAsync(PaymentCompletedEvent e)
        {
            await _notification.SendPaymentPaidAsync(e.BookingId);
            
            await _signalR.SendDashboardUpdateAsync(SignalREvents.PaymentPaid, new { paymentId = e.PaymentId, bookingId = e.BookingId });
        }
    }
}
