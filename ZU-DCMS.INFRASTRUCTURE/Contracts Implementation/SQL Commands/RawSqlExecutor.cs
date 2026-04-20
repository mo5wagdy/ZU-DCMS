using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.ContractImplementation
{
    // _________________________ Raw SQL Executor _________________________ //

    // __ This class provides a way to execute raw SQL commands against the database using Entity Framework Core. __ //
    public class RawSqlExecutor : IRawSqlExecutor
    {
        private readonly AppDbContext _context;
        public RawSqlExecutor(AppDbContext context)
        {
            _context = context;
        }

        // __ Executes a raw SQL command asynchronously. __ //
        // __ If parameters are provided, they are converted to SqlParameter objects and passed to the ExecuteSqlRawAsync method. __ //
        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            // __ If no parameters are provided, execute the SQL command directly. __ //
            if (parameters == null)
                return await _context.Database.ExecuteSqlRawAsync(sql);

            // __ Convert the parameters object to an array of SqlParameter objects. __ //
            var sqlParams = parameters.GetType()
                .GetProperties()
                .Select(p => new SqlParameter($"@{p.Name}", p.GetValue(parameters) ?? DBNull.Value))
                .ToArray();

            // __ Execute the SQL command with the provided parameters. __ //
            return await _context.Database.ExecuteSqlRawAsync(sql, sqlParams);
        }
    }
}

