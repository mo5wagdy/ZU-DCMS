
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.ContractImplementation
{
    // _________________________ User Code Generator Implementation _________________________ //
    public class UserCodeGenerator : IUserCodeGenerator
    {
        private readonly IRawSqlExecutor _sql;

        public UserCodeGenerator(IRawSqlExecutor sql)
        {
            _sql = sql;
        }

        // __ Generate a unique code for a user based on the provided prefix and sequence name __ //
        public async Task<string> GenerateAsync(string prefix, string sequenceName)
        {
            var seq = await _sql.ExecuteScalarAsync<long>($"SELECT NEXT VALUE FOR {sequenceName}");

            return $"{prefix}-{DateTime.UtcNow.Year}-{seq:D4}";
        }
    }
}
