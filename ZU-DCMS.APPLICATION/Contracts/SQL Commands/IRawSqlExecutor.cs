
namespace ZU_DCMS.APPLICATION.Contracts
{
    // __ This interface defines a contract for executing raw SQL queries and retrieving results. __ //
    public interface IRawSqlExecutor
    {
        // __ This method is responsible for executing a raw SQL query and returning a single value of Type T. __ //
        Task<int> ExecuteAsync(string sql, object? parameters = null);

        // __ We use sql query with type int or long so the constraint is set to struct __ //
        Task<T> GetScalarAsync<T>(string sequenceName);

        // __ Executes a raw SQL query and returns a single scalar value (e.g., Id) within the current transaction. __ //
        Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null);
    }
}