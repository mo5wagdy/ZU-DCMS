using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Bookings.Commands.RequeueOverdueBookings
{
    public record RequeueOverdueBookingsCommand : IRequest<Result<int>>;

    public class RequeueOverdueBookingsHandler : IRequestHandler<RequeueOverdueBookingsCommand, Result<int>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRawSqlExecutor _sql;
        private readonly IAppLogger<RequeueOverdueBookingsHandler> _logger;

        public RequeueOverdueBookingsHandler
        (
            IUnitOfWork uow,
            IRawSqlExecutor sql,
            IAppLogger<RequeueOverdueBookingsHandler> logger
        )
        {
            _uow = uow;
            _sql = sql;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(RequeueOverdueBookingsCommand command, CancellationToken cancellationToken)
        {
            var nowTime = DateTime.Now.TimeOfDay;
            var todayDate = DateTime.Today;

            var gracePeriod = TimeSpan.FromMinutes(5);
            var reQueueTime = nowTime.Subtract(gracePeriod);

            var overdueBookings = await _uow.Repository<Booking>().GetListAsync
            (b =>
                   b.Session.Date.Date == todayDate &&
                   b.Status == BookingStatus.Confirmed &&
                   b.Session.EndTime < reQueueTime,
                   includes: b => b.Session
            );

            if (!overdueBookings.Any()) return Result.Success(0);

            _logger.LogInfo("Found {Count} overdue bookings to re-queue", overdueBookings.Count);

            int movedCount = 0;

            await _uow.BeginTransactionAsync();

            try
            {
                foreach (var booking in overdueBookings)
                {
                    if (booking.Session is null)
                    {
                        _logger.LogWarning("Booking{Id} has null session", booking.Id);
                        continue;
                    }

                    // __ increment to new session __ //
                    var bookingTypeValue = booking.BookingType == BookingType.New ? 1 : 2;

                    // __ Construct the SQL query to increment the count if it is less than max and the session is active and not deleted __ //
                    var incrementSql = @"
                     DECLARE @Updated TABLE (Id INT);
                     
                     UPDATE TOP (1) Sessions
                     SET 
                         CurrentNewCount = CASE 
                             WHEN @type = 1 THEN CurrentNewCount + 1 
                             ELSE CurrentNewCount 
                         END,
                         CurrentFollowUpCount = CASE 
                             WHEN @type = 2 THEN CurrentFollowUpCount + 1 
                             ELSE CurrentFollowUpCount 
                         END
                     OUTPUT INSERTED.Id INTO @Updated
                     WHERE 
                         Date = @today
                         AND EndTime > @now
                         AND IsActive = 1
                         AND IsDeleted = 0
                         AND (
                             (@type = 1 AND CurrentNewCount < MaxNewPatients) OR
                             (@type = 2 AND CurrentFollowUpCount < MaxFollowUpPatients)
                         )
                     ORDER BY Date, StartTime;
                     
                     SELECT TOP 1 Id FROM @Updated;
";

                    // __ Execute the SQL query and check how many rows were incrementAffected __ //
                    var newSessionId = await _sql.QuerySingleAsync<int?>
                    (
                        incrementSql,
                        new
                        {
                            today = todayDate,
                            now = nowTime,
                            type = bookingTypeValue
                        }
                    );

                    // __ If no rows were incrementAffected, it means no available session was found __ //
                    if (newSessionId != null)
                    {
                        // __ Decrement from old session __ //
                        var decrementColumn = booking.BookingType == BookingType.New ? nameof(Session.CurrentNewCount) : nameof(Session.CurrentFollowUpCount);

                        // __ Construct the SQL query to decrement the count if it is greater than zero and the session is active and not deleted __ //
                        var decrementSql = $@"
                        UPDATE Sessions
                        SET {decrementColumn} = {decrementColumn} - 1
                        WHERE Id = @id
                        AND {decrementColumn} > 0
                        AND IsActive = 1
                        AND IsDeleted = 0";

                        // __ Execute the SQL query and check how many rows were decrementAffected __ //
                        var decrementAffected = await _sql.ExecuteAsync(decrementSql, new { id = booking.SessionId });

                        // __ If no rows were decrementAffected, it means the session was either inactive, deleted, does not exist, or the count was already at zero __ //
                        if (decrementAffected == 0)
                        {
                            await _uow.RollbackTransactionAsync();

                            _logger.LogWarning("Failed to release slot for SessionId: {SessionId}, Type: {Type}", booking.SessionId, booking.BookingType);

                            return Result.Failure<int>("Failed to release booking slot");
                        }

                        booking.SessionId = newSessionId.Value;
                        booking.Status = BookingStatus.Delayed;
                        _uow.Repository<Booking>().Update(booking);

                        movedCount++;
                    }
                    else
                    {
                        booking.Status = BookingStatus.Cancelled;
                        booking.PostponeReason = "Cancelled due to no-show and no other appointments available today";
                        _uow.Repository<Booking>().Update(booking);
                    }
                }

                await _uow.SaveChangesAsync(cancellationToken: cancellationToken);

                await _uow.CommitTransactionAsync();

                return Result.Success(movedCount);
            }
            catch
            {
                await _uow.RollbackTransactionAsync();

                return Result.Failure<int>("An error occurred while rescheduling bookings");
            }
        }
    }
}