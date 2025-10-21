using Application.Interfaces;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class AzureServiceBusService : IMessageBusService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<AzureServiceBusService> _logger;

        public AzureServiceBusService(ServiceBusClient serviceBusClient, ILogger<AzureServiceBusService> logger)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SubscribeAsync<TMessage>(string topic, Func<TMessage, Task> onMessage, CancellationToken cancellationToken = default)
        {
            var processor = _serviceBusClient.CreateProcessor(topic);

            processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();
                var message = System.Text.Json.JsonSerializer.Deserialize<TMessage>(body);

                if (message != null)
                {
                    await onMessage(message);
                }

                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error processing message");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(cancellationToken);
        }

        public async Task PublishAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default)
        {
            var sender = _serviceBusClient.CreateSender(topic);

            try
            {
                var serviceBusMessage = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(message))
                {
                    ContentType = "application/json"
                };

                await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }
    }
}
