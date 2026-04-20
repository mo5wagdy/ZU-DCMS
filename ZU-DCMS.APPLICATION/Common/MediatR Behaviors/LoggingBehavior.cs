
using MediatR;
using ZU_DCMS.APPLICATION.Contracts.Logger;

namespace ZU_DCMS.APPLICATION.Common.MediatR_Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : Result
    {
        private readonly IAppLogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(IAppLogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Handling {Request}", typeof(TRequest).Name);

            var response = await next();

            _logger.LogInfo("Handled {Request}", typeof(TRequest).Name);

            return response;
        }
    }
}
