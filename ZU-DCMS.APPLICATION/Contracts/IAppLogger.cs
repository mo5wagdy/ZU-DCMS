
namespace ZU_DCMS.APPLICATION.Contracts
{
    // __ Interface for application logging to provide a consistent logging mechanism across the application __ //
    public interface IAppLogger<T>
    {
        void LogInfo(string message, params object[] args); // => Method to log informational messages with optional formatting arguments
        void LogWarning(string message, params object[] args); // => Method to log warning messages with optional formatting arguments
        void LogError(string message, Exception? ex = null, params object[] args); // => Method to log error messages with an optional exception and formatting arguments
    } 
}