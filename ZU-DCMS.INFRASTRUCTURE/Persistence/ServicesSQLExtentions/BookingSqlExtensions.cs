using Microsoft.Data.SqlClient;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.ServicesSQLExtentions
{
    // __ This static class contains extension methods for executing raw SQL queries related to booking operations. __ //
    public static class BookingSqlExtensions
    {
        /* 
         * This method attempts to reserve a session slot for a given session ID and booking type.
         * It updates the current count of either new or follow-up patients based on the booking type,
         * ensuring that the count does not exceed the maximum allowed and the race condition is handled. 
         * The method returns the number of rows affected by the update operation,
         * which indicates whether the reservation was successful.
         */
        public static async Task<int> TryReserveSessionSlotAsync(this IRawSqlExecutor executor, int sessionId, BookingType type)
        {
            string targetSlotCount = type == BookingType.New ? nameof(Session.CurrentNewCount) : nameof(Session.CurrentFollowUpCount);
            string targetSlotLimit = type == BookingType.New ? nameof(Session.MaxNewPatients) : nameof(Session.MaxFollowUpPatients);

            string sessionCapacityUpdateSql = $@"
            UPDATE Sessions 
            SET {targetSlotCount} = {targetSlotCount} + 1 
            WHERE Id = @id AND {targetSlotCount} < {targetSlotLimit};
            SELECT @@ROWCOUNT;";

            return await executor.ExecuteAsync<int>(sessionCapacityUpdateSql, new SqlParameter("@id", sessionId));
        }
    }
}
