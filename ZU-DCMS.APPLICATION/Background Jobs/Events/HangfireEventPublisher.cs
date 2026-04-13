using Hangfire;

namespace ZU_DCMS.APPLICATION.Background_Jobs.Events
{
    public class HangfireEventPublisher : IEventPublisher
    {
        public Task PublishAsync(IDomainEvent domainEvent)
        {
            BackgroundJob.Enqueue<EventDispatcher>(x => x.Dispatch(domainEvent));

            return Task.CompletedTask;
        }
    }
}
