using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        public async Task<T> GetScalarAsync<T>(string sequenceName)
        {
            // __ ADO.NET Call __ //
            var connection = _context.Database.GetDbConnection();

            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT NEXT VALUE FOR {sequenceName}";

            // __ Making the command use the currently opening transaction __ //
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result is null || result == DBNull.Value)
            {
                throw new InvalidOperationException($"Coud not get next value for sequence {sequenceName}");
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
        {
            // __ ADO.NET Call __ //
            var connection = _context.Database.GetDbConnection();

            using var command = connection.CreateCommand();

            command.CommandText = sql;

            // __ Making the command use the currently opening transaction __ //
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            // __ Add parameters if provided __ //
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var value = prop.GetValue(parameters) ?? DBNull.Value;

                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = value;

                    command.Parameters.Add(param);
                }
            }

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result is null || result == DBNull.Value)
                return default;

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}