
using Microsoft.Extensions.DependencyInjection;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Events
{
    /* 
     * This class is responsible for dispatching domain events to their respective handlers.
     * It uses reflection to find and invoke the appropriate handler methods for each event Type.
     * The handlers are resolved from the dependency injection container, allowing for loose coupling and easy extensibility.
     */
    public class EventDispatcher
    {
        // __ The IServiceProvider is used to resolve event handlers from the dependency injection container. __ // 
        private readonly IServiceProvider _provider;

        public EventDispatcher(IServiceProvider provider)
        {
            _provider = provider;
        }

        // __ This method dispatches a given domain event to all registered handlers for that event Type. __ //
        public async Task Dispatch(IDomainEvent domainEvent)
        {
            // __ The handler Type is determined by creating a generic Type based on the event's Type. __ //
            var handlerType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());

            // __ All handlers for the event Type are resolved from the service provider. __ //
            var handlers = _provider.GetServices(handlerType);

            // __ Each handler's HandleAsync method is invoked with the domain event as an argument. __ //
            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");

                await (Task)method!.Invoke(handler, new object[] { domainEvent })!;
            }
        }
    }
}
