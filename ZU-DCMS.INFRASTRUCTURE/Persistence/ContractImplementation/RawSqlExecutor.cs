using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.INFRASTRUCTURE.Persistence;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.ContractImplementation
{
    // _________________________ Raw SQL Executor _________________________ //
    public class RawSqlExecutor : IRawSqlExecutor
    {
        private readonly AppDbContext _context;

        public RawSqlExecutor(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Executes a raw SQL query and returns a single value of the specified type.
        /// It is the caller's responsibility to ensure that the SQL query returns a single value and that it can be cast to the specified type.
        /// </summary>

        public async Task<T> ExecuteScalarAsync<T>(string sql, params DbParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = sql;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                        command.Parameters.Add(param);
                }

                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                    return default!;

                return result is T value? value : (T)Convert.ChangeType(result, typeof(T));
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
