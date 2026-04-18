using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Booking.Commands.CancelBooking
{
    public class CancelBookingHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly ISessionService _sessionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAppLogger<CancelBookingHandler> _logger;

        public CancelBookingHandler(
            IUnitOfWork uow,
            ISessionService sessionService,
            IEventPublisher eventPublisher,
            IAppLogger<CancelBookingHandler> logger)
        {
            _uow = uow;
            _sessionService = sessionService;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<Result> Handle(CancelBookingCommand command)
        {
            _logger.LogInfo("Cancelling booking {BookingId} for PatientId: {PatientId}", command.BookingId, command.PatientId);

            var booking = await _uow.Repository<Domain.Entities.Booking>().GetFirstOrDefaultAsync
                 (
                    b => b.Id == command.BookingId,
                    false,
                    b => b.Patient,
                    b => b.Session,
                    b => b.Payment
                 );

            if (booking is null)
            {
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", command.BookingId);
                return Result.Failure("الحجز غير موجود");
            }

            if (booking.PatientId != command.PatientId)
            {
                _logger.LogWarning("Unauthorized cancellation attempt for BookingId: {BookingId} by PatientId: {PatientId}", command.BookingId, command.PatientId);
                return Result.Failure("غير مصرح");
            }

            if (booking.Payment?.Status == PaymentStatus.Paid)
            {
                _logger.LogWarning("Attempt to cancel paid booking {BookingId}", command.BookingId);
                return Result.Failure("لا يمكن إلغاء الحجز بعد الدفع");
            }

            await _uow.BeginTransactionAsync();

            try
            {
                booking.Status = BookingStatus.Cancelled;
                _uow.Repository<Domain.Entities.Booking>().Update(booking);

                var release = await _sessionService.ReleaseSlotAsync(booking.SessionId, booking.BookingType);

                if (release.IsFailure)
                {
                    await _uow.RollbackTransactionAsync();
                    _logger.LogError("Failed to release slot during cancellation of BookingId");
                    return Result.Failure(release.Error);
                }

                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Booking cancelled {BookingId}", command.BookingId);

                await _eventPublisher.PublishAsync(new BookingCancelledEvent(booking.Id, booking.SessionId));

                return Result.Success();
            }

            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                _logger.LogError("Error cancelling booking", ex);
                return Result.Failure("فشل الإلغاء");
            }
        }
    }
}
