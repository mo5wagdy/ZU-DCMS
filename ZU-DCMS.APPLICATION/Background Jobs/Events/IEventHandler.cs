
namespace ZU_DCMS.APPLICATION.Background_Jobs.Events
{
    // __ This interface defines a contract for handling domain events. It is generic, allowing for different types of events to be handled by implementing classes. __ //
    public interface IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        // __ This method handles the given domain event asynchronously. __ //
        Task HandleAsync(TEvent domainEvent);
    }
}
