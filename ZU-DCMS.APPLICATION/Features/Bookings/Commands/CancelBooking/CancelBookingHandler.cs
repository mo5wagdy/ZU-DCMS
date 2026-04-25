using MediatR;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingHandler : IRequestHandler<CancelBookingCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRawSqlExecutor _sql;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAppLogger<CancelBookingHandler> _logger;

        public CancelBookingHandler(
            IUnitOfWork uow,
            IRawSqlExecutor sql,
            IEventPublisher eventPublisher,
            IAppLogger<CancelBookingHandler> logger)
        {
            _uow = uow;
            _sql = sql;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        // __ Cancel an existing booking __ //
        public async Task<Result> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Cancelling booking {BookingId} for PatientId: {PatientId}", command.BookingId, command.PatientId);

            // __ Load booking with related entities __ //
            var booking = await _uow.Repository<Booking>().GetFirstOrDefaultAsync
                 (
                    b => b.Id == command.BookingId,
                    false,
                    b => b.Patient,
                    b => b.Session
                 );

            // __ Check if booking exists __ //
            if (booking is null)
            {
                _logger.LogWarning("Booking not found for BookingId: {BookingId}", command.BookingId);
            
                return Result.Failure("الحجز غير موجود");
            }

            // __ Check if the booking belongs to the patient __ //
            if (booking.PatientId != command.PatientId)
            {
                _logger.LogWarning("Unauthorized cancellation attempt for BookingId: {BookingId} by PatientId: {PatientId}", command.BookingId, command.PatientId);
                
                return Result.Failure("غير مصرح");
            }

            // __ Transaction for cancellation process __ //
            await _uow.BeginTransactionAsync();

            try
            {
                // __ Update booking status to Cancelled __ //
                booking.Status = BookingStatus.Cancelled;
                
                _uow.Repository<Booking>().Update(booking);

                // __ Release the reserved slot in the session __ //
                _logger.LogInfo("Releasing slot for Session Type: {Type}", booking.BookingType);

                // __ We will use a raw SQL update to decrement the count atomically and ensure we do not go below zero __ //

                // __ Determine which column to update based on booking Type __ //
                var column = booking.BookingType == BookingType.New ? nameof(Session.CurrentNewCount) : nameof(Session.CurrentFollowUpCount);

                // __ Construct the SQL query to decrement the count if it is greater than zero and the session is active and not deleted __ //
                var sql = $@"
                UPDATE Sessions
                SET {column} = {column} - 1
                WHERE Id = @id
                AND {column} > 0
                AND IsActive = 1
                AND IsDeleted = 0";

                // __ Execute the SQL query and check how many rows were affected __ //
                var affected = await _sql.ExecuteAsync(sql, new { id = booking.SessionId }); 

                // __ If no rows were affected, it means the session was either inactive, deleted, does not exist, or the count was already at zero __ //
                if (affected == 0)
                {
                    await _uow.RollbackTransactionAsync();

                    _logger.LogWarning("Failed to release slot for SessionId: {SessionId}, Type: {Type}", booking.SessionId, booking.BookingType);

                    return Result.Failure("فشل تحرير مكان الحجز");
                }

                // __ Save changes to database __ //
                await _uow.CommitTransactionAsync();

                _logger.LogInfo("Booking cancelled {BookingId}", command.BookingId);

                // __ Publish domain event for booking cancellation __ //
               // await _eventPublisher.PublishAsync(new BookingCancelledEvent(booking.Id, booking.SessionId));

                return Result.Success();
            }

            // __ Catch any exceptions, log error, rollback transaction, and return failure __ //
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
               
                _logger.LogError("Error cancelling booking", ex);
                
                return Result.Failure("فشل الإلغاء");
            }
        }
    }
}