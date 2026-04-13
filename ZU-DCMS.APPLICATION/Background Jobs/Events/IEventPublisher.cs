
namespace ZU_DCMS.APPLICATION.Background_Jobs.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync(IDomainEvent domainEvent);
    }
}
