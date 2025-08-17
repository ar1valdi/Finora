using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Finora.Web.Configuration;
using Microsoft.Extensions.Options;
using Finora.Messages.Interfaces;

namespace Finora.Web.Services
{
    public interface IRabbitMqService
    {
        Task<Guid >PublishMessage(string queueName, IMessageWithCorrelationId message);
        Task EnsureQueueExists(string queueName);
    }

    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMqService> _logger;
        private bool _disposed = false;

        public RabbitMqService(IOptions<RabbitMqConfiguration> configuration, ILogger<RabbitMqService> logger)
        {
            _logger = logger;
            
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration.Value.HostName,
                    Port = configuration.Value.Port,
                    UserName = configuration.Value.UserName,
                    Password = configuration.Value.Password,
                    VirtualHost = configuration.Value.VirtualHost
                };

                _connection = factory.CreateConnectionAsync().Result;
                _channel = _connection.CreateChannelAsync().Result;
                
                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                throw;
            }
        }

        public async Task<Guid> PublishMessage(string queueName, IMessageWithCorrelationId message)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqService));

            try
            {
                await EnsureQueueExists(queueName);

                if (message.CorrelationId == Guid.Empty)
                {
                    message.CorrelationId = Guid.NewGuid();
                }

                var correlationId = message.CorrelationId;

                var messageBody = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageBody);

                var properties = new BasicProperties();
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Persistent = true;

                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Message {MessageId} published to queue {QueueName}", correlationId, queueName);
                
                return correlationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
                throw;
            }
        }

        public async Task EnsureQueueExists(string queueName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqService));

            try
            {
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogDebug("Queue {QueueName} ensured to exist", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure queue {QueueName} exists", queueName);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _channel?.CloseAsync().Wait();
                _channel?.Dispose();
                _connection?.CloseAsync().Wait();
                _connection?.Dispose();
                _disposed = true;
                
                _logger.LogInformation("RabbitMQ connection disposed");
            }
        }
    }
}
