using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Features.Session.Commands.ReserveSlot
{
    public class ReserveSlotHandler
    {
        private readonly IRawSqlExecutor _sql;
        private readonly IAppLogger<ReserveSlotHandler> _logger;

        public ReserveSlotHandler(IRawSqlExecutor sql, IAppLogger<ReserveSlotHandler> logger)
        {
            _sql = sql;
            _logger = logger;
        }

        public async Task<Result> Handle(ReserveSlotCommand command)
        {
            var sessionId = command.SessionId;
            var type = command.Type;

            _logger.LogInfo("Reserving slot for SessionId: {SessionId}, Type: {Type}", sessionId, type);

            var column = type == BookingType.New ? nameof(Domain.Entities.Session.CurrentNewCount) : nameof(Domain.Entities.Session.CurrentFollowUpCount);

            var maxColumn = type == BookingType.New ? nameof(Domain.Entities.Session.MaxNewPatients) : nameof(Domain.Entities.Session.MaxFollowUpPatients);

            var sql = $@"
            UPDATE Sessions
            SET {column} = {column} + 1
            WHERE Id = @id
            AND {column} < {maxColumn}
            AND IsActive = 1
            AND IsDeleted = 0";

            var affected = await _sql.ExecuteAsync(sql, new { id = sessionId });

            if (affected == 0)
            {
                _logger.LogWarning("Failed to reserve slot for SessionId: {SessionId}, BookingType: {Type}", sessionId, type);
                return Result.Failure("السكشن غير متاح أو تم حجزه للتو، حاول اختيار موعد آخر");
            }

            return Result.Success();
        }
    }
}
