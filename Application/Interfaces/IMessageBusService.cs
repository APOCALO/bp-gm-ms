namespace Application.Interfaces
{
    public interface IMessageBusService
    {
        Task SubscribeAsync<TMessage>(string topic, Func<TMessage, Task> onMessage, CancellationToken cancellationToken = default);
        Task PublishAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default);
    }
}
