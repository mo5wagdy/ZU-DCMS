using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.ContractImplementation
{
    public class UserCodeGenerator : IUserCodeGenerator
    {
        private readonly IRawSqlExecutor _sql;

        public UserCodeGenerator(IRawSqlExecutor sql)
        {
            _sql = sql;
        }

        public async Task<string> GenerateAsync(string prefix, string sequenceName)
        {
            var seq = await _sql.ExecuteScalarAsync<long>($"SELECT NEXT VALUE FOR {sequenceName}");

            return $"{prefix}-{DateTime.UtcNow.Year}-{seq:D4}";
        }
    }
}
