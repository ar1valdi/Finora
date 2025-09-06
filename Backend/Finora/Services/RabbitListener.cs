using System.Text;
using System.Text.Json;
using Finora.Backend.Models.Concrete;
using Finora.Messages.Interfaces;
using Finora.Messages.Wrappers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Finora.Web.Services;

public interface IRabbitListener
{
    public Task Listen(string queueName, CancellationToken cancellationToken);
}

public class RabbitListener(
    IRabbitMqService rabbitMqService,
    ILogger<RabbitListener> logger
) : IRabbitListener
{
    private IChannel? _channel;

    public async Task Listen(string queueName, CancellationToken cancellationToken)
    {
        _channel = await rabbitMqService.GetChannel(cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                logger.LogInformation("Received message from queue {QueueName}", queueName);

                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var envelope = JsonSerializer.Deserialize<MessageEnvelope>(message);

                if (envelope is null)
                {
                    logger.LogError("Failed to deserialize message from queue {QueueName}", queueName);
                    return;
                }

                var messageId = envelope.MessageId;
                var correlationId = args.BasicProperties.CorrelationId;
                var replyTo = args.BasicProperties.ReplyTo;

                logger.LogInformation("Message details:\n" +
                    "CorrelationId: {CorrelationId}\n" +
                    "ReplyTo: {ReplyTo}\n" +
                    "MessageId: {MessageId}\n" +
                    "Body: {Body}", correlationId, replyTo, messageId, envelope.Data);

                var response = new RabbitResponse<string>
                {
                    Data = $"Response to message with correlation id {correlationId ?? "<no correlation id>"} and message id {messageId?.ToString() ?? "<no message id>"}",
                    CorrelationId = correlationId == null ? Guid.Empty : Guid.Parse(correlationId),
                    MessageId = Guid.NewGuid()
                };

                if (replyTo is not null)
                {
                    await rabbitMqService.PublishMessage(replyTo, response, _channel, cancellationToken);
                }

                await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);

                logger.LogInformation("Message corr-{CorrelationId} acknowledged.", correlationId);
            }
            catch (Exception ex)
            {
                await _channel.BasicNackAsync(args.DeliveryTag, false, true, cancellationToken);
                logger.LogError(ex, "Failed to process message from queue {QueueName}. DeliveryTag: {DeliveryTag}", queueName, args.DeliveryTag);
            }
        };

        logger.LogInformation("Listening to {QueueName}...", queueName);
        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);
    }
}

