
using Microsoft.Extensions.Logging;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.ContractImplementation
{
    // __ Logger implementation to provide application-specific logging functionality __ //
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public AppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        // __ Method to log informational messages with optional formatting arguments __ //
        public void LogInfo(string message, params object[] args)
            => _logger.LogInformation(message, args);

        // __ Method to log warning messages with optional formatting arguments __ //
        public void LogWarning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        // __ Method to log error messages with an optional exception and formatting arguments __ //
        public void LogError(string message, Exception? ex = null, params object[] args)
            => _logger.LogError(ex, message, args);
    }
}
