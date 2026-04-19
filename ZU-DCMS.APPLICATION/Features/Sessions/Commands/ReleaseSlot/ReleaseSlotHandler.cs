using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.APPLICATION.Features.Session.Commands.ReleaseSlot
{
    public class ReleaseSlotHandler
    {
        private readonly IRawSqlExecutor _sql;
        private readonly IAppLogger<ReleaseSlotHandler> _logger;

        public ReleaseSlotHandler(IRawSqlExecutor sql, IAppLogger<ReleaseSlotHandler> logger)
        {
            _sql = sql;
            _logger = logger;
        }

        public async Task<Result> Handle(ReleaseSlotCommand command)
        {
            var sessionId = command.SessionId;
            var type = command.Type;

            _logger.LogInfo("Releasing slot for SessionId: {SessionId}, Type: {Type}", sessionId, type);

            var column = type == BookingType.New ? nameof(Domain.Entities.Session.CurrentNewCount) : nameof(Domain.Entities.Session.CurrentFollowUpCount);

            var sql = $@"
            UPDATE Sessions
            SET {column} = {column} - 1
            WHERE Id = @id
            AND {column} > 0
            AND IsActive = 1
            AND IsDeleted = 0";

            var affected = await _sql.ExecuteAsync(sql, new { id = sessionId });

            if (affected == 0)
            {
                _logger.LogWarning("Failed to release slot for SessionId: {SessionId}, Type: {Type}", sessionId, type);
                return Result.Failure("فشل تحرير مكان الحجز");
            }

            return Result.Success();
        }
    }
}
