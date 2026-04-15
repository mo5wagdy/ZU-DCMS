
namespace ZU_DCMS.APPLICATION.Contracts
{
    // __ This interface defines a contract for executing raw SQL queries and retrieving results. __ //
    public interface IRawSqlExecutor
    {
        // __ This method is responsible for executing a raw SQL query and returning a single value of type T. __ //
        Task<int> ExecuteAsync(string sql, object? parameters = null);
    }
}
