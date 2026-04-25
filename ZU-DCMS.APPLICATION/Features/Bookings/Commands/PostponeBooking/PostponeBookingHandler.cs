//using MediatR;
//using ZU_DCMS.APPLICATION.Background_Jobs.Events;
//using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events;
//using ZU_DCMS.APPLICATION.Common;
//using ZU_DCMS.APPLICATION.Contracts.Logger;
//using ZU_DCMS.Domain.Entities;
//using ZU_DCMS.Domain.Enums;
//using ZU_DCMS.Domain.Interfaces;

//namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.PostponeBooking
//{
//    public class PostponeBookingHandler : IRequestHandler<PostponeBookingCommand, Result>
//    {
//        private readonly IUnitOfWork _uow;
//        private readonly IEventPublisher _eventPublisher;
//        private readonly IAppLogger<PostponeBookingHandler> _logger;

//        public PostponeBookingHandler(
//            IUnitOfWork uow,
//            IEventPublisher eventPublisher,
//            IAppLogger<PostponeBookingHandler> logger)
//        {
//            _uow = uow;
//            _eventPublisher = eventPublisher;
//            _logger = logger;
//        }

//        // __ Postpone an existing booking to a new session __ //
//        public async Task<Result> Handle(PostponeBookingCommand command, CancellationToken cancellationToken)
//        {
//            _logger.LogInfo("Postponing booking {BookingId} by AdminId: {AdminId} with reason: {Reason}", command.BookingId, command.AdminId, command.Reason);

//            // __ Load booking with related entities __ //
//            var booking = await _uow.Repository<Booking>().GetFirstOrDefaultAsync
//                 (
//                    b => b.Id == command.BookingId,
//                    false,
//                    b => b.Patient,
//                    b => b.Session
//                 );

//            // __ Check if booking exists __ //
//            if (booking is null)
//            {
//                _logger.LogWarning("Booking not found for BookingId: {BookingId}", command.BookingId);
                
//                return Result.Failure("الحجز غير موجود");
//            }

//            // __ Get available slots for the booking Type __ //
//           // var slots = await _sessionService.GetAvailableSlotsAsync(booking.BookingType);

//            if (slots.IsFailure || slots.Value.Count == 0)
//            {
//                _logger.LogWarning("No available slots found for BookingType: {BookingType} during postponement of BookingId", booking.BookingType);
//                return Result.Failure("لا يوجد مواعيد");
//            }

//            var newSessionId = slots.Value.First().SessionId;

//            await _uow.BeginTransactionAsync();

//            try
//            {
//                var release = await _sessionService.ReleaseSlotAsync(booking.SessionId, booking.BookingType);

//                if (release.IsFailure)
//                {
//                    await _uow.RollbackTransactionAsync();
//                    _logger.LogError("Failed to release old slots");
//                    return Result.Failure(release.Error);
//                }

//                var reserve = await _sessionService.ReserveSlotAsync(newSessionId, booking.BookingType);

//                if (reserve.IsFailure)
//                {
//                    await _uow.RollbackTransactionAsync();
//                    _logger.LogError("Failed to reserve new slot for Session during postponement");
//                    return Result.Failure(reserve.Error);
//                }

//                booking.SessionId = newSessionId;
//                booking.Status = BookingStatus.Postponed;
//                booking.PostponeReason = command.Reason.Trim();
//                booking.PostponedAt = DateTime.UtcNow;

//                _uow.Repository<Domain.Entities.Booking>().Update(booking);

//                await _uow.CommitTransactionAsync();

//                _logger.LogInfo("Booking postponed {BookingId}", command.BookingId);

//                await _eventPublisher.PublishAsync(new BookingPostponedEvent(booking.Id, booking.SessionId, command.Reason));

//                return Result.Success();
//            }

//            catch (Exception ex)
//            {
//                _logger.LogError("Error postponing booking", ex);
//                await _uow.RollbackTransactionAsync();
//                return Result.Failure("فشل التأجيل");
//            }
//        }
//    }
//}
