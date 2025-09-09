using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Finora.Web.Configuration;
using Microsoft.Extensions.Logging;
using Finora.Messages.Interfaces;
using Microsoft.Extensions.Options;
using Finora.Kernel;
using Finora.Backend.Common;

namespace Finora.Web.Services
{
    public interface IRabbitMqService
    {
        Task<(Guid messageId, Guid correlationId)> PublishMessage(
            string exchangeName,
            string routingKey,
            Guid correlationId,
            object message,
            IChannel channel,
            CancellationToken ct);
        Task<(Guid messageId, Guid correlationId)> PublishMessage(
            string queueName,
            Guid correlationId,
            object message,
            IChannel channel,
            CancellationToken ct);

        Task<(Guid messageId, Guid correlationId)> PublishFromJson(
            string exchangeName,
            string routingKey,
            Guid correlationId,
            Guid messageId,
            string jsonMessage,
            IChannel channel,
            CancellationToken ct);

        Task<IChannel> GetChannel(CancellationToken ct);
        Task EnsureTopology(CancellationToken ct);
    }

    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private IConnection? _connection;
        private bool _disposed = false;
        private readonly RabbitMqConfiguration _configuration;
        
        public RabbitMqService(ILogger<RabbitMqService> logger, IOptions<RabbitMqConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration.HostName,
                    Port = _configuration.Port,
                    UserName = _configuration.UserName,
                    Password = _configuration.Password,
                    VirtualHost = _configuration.VirtualHost
                };

                _connection = factory.CreateConnectionAsync().Result;

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                throw;
            }
        }

        public async Task<IChannel> GetChannel(CancellationToken ct)
        {
            if (_connection is null)
                throw new Exception("Connection is not established");

            return await _connection.CreateChannelAsync(null, ct);
        }

        public async Task<(Guid messageId, Guid correlationId)> PublishMessage(
            string queueName,
            Guid correlationId,
            object message,
            IChannel channel,
            CancellationToken ct)
        {
            return await PublishMessage(string.Empty, queueName, correlationId, message, channel, ct);
        }

        public async Task<(Guid messageId, Guid correlationId)> PublishMessage(
            string exchangeName,
            string routingKey,
            Guid correlationId,
            object message,
            IChannel channel,
            CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqService));

            var msgWithCorrId = message as IMessage;

            if (msgWithCorrId is null)
                throw new ArgumentException("Message must implement IMessageWithCorrelationId", nameof(message));
            var messageId = msgWithCorrId.MessageId == Guid.Empty ? Guid.NewGuid() : msgWithCorrId.MessageId;
            msgWithCorrId.MessageId = messageId;

            try
            {
                var messageBody = JsonSerializer.Serialize(message, JsonConfig.JsonOptions);
                await PublishFromJson(exchangeName, routingKey, correlationId, messageId, messageBody, channel, ct);
                return (messageId, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Msg publish failed: msgId={MessageId}, corrId={CorrelationId}, exchange={ExchangeName}, routing key={RoutingKey}",
                    messageId, correlationId, exchangeName, routingKey);
                throw;
            }
        }

        public async Task<(Guid messageId, Guid correlationId)> PublishFromJson(
            string exchangeName,
            string routingKey,
            Guid correlationId,
            Guid messageId,
            string jsonMessage,
            IChannel channel,
            CancellationToken ct)
        {
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = new BasicProperties();
            properties.CorrelationId = correlationId.ToString();
            properties.MessageId = messageId.ToString();
            properties.Persistent = true;

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);

            _logger.LogInformation(
                "Message published: msgId={MessageId}, corrId={CorrelationId}, exchange={QueueName}, routing key={RoutingKey}",
                messageId, correlationId, exchangeName, routingKey);

            return (messageId, correlationId);
        }

        public async Task EnsureTopology(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqService));

            var channel = await GetChannel(ct);

            await channel.ExchangeDeclareAsync(
                    _configuration.RequestExchange,
                    ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: ct);

            await channel.ExchangeDeclareAsync(
                    _configuration.InternalExchangeName,
                    ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: ct);

            string commandsQueue = _configuration.RequestQueue;
            string queriesQueue = _configuration.QueryQueue;

            await channel.QueueDeclareAsync(
                commandsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);
            await channel.QueueDeclareAsync(
                queriesQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            await channel.QueueDeclareAsync(
                "DEBUG_RESPONSES",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            await channel.QueueBindAsync(
                commandsQueue,
                exchange: _configuration.RequestExchange,
                routingKey: _configuration.CommandRoutingKey,
                cancellationToken: ct
            );
            await channel.QueueBindAsync(
                queriesQueue,
                exchange: _configuration.RequestExchange,
                routingKey: _configuration.QueryRoutingKey,
                cancellationToken: ct
            );

            await channel.DisposeAsync();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.CloseAsync().Wait();
                _connection?.Dispose();
                _disposed = true;

                _logger.LogInformation("RabbitMQ connection disposed");
            }
        }
    }
}
