using System.Text;
using System.Text.Json;
using Finora.Backend.Models.Concrete;
using Finora.Messages.Wrappers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MediatR;

namespace Finora.Web.Services;

public interface IRabbitListener
{
    public Task Listen(string queueName, CancellationToken cancellationToken);
}

public class RabbitListener(
    IRabbitMqService rabbitMqService,
    ILogger<RabbitListener> logger,
    IMediator mediator
) : IRabbitListener
{
    private IChannel? _channel;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

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
                var envelope = JsonSerializer.Deserialize<MessageEnvelope>(message, JsonOptions);

                if (envelope is null)
                {
                    throw new InvalidDataException($"Failed to deserialize message from queue {queueName}");
                }

                var messageId = envelope.MessageId;
                var correlationId = args.BasicProperties.CorrelationId;
                var replyTo = args.BasicProperties.ReplyTo;
                var msgType = envelope.Type;
                var cqsType = envelope.MessageType.ToString();

                logger.LogInformation("Message details:\n" +
                    "Correlation Id: {CorrelationId}\n" +
                    "Reply To: {ReplyTo}\n" +
                    "Message Id: {MessageId}\n" +
                    "Message Type: {MessageType}\n" +
                    "CQS Type: {cqsType}\n" +
                    "Body: {Body}", correlationId, replyTo, messageId, msgType, cqsType, envelope.Data);

                var request = MessageMapper.DeserializeMessage(msgType, envelope.Data.ToString() ?? "{}");
                
                var response = await mediator.Send(request, cancellationToken);

                var rabbitResponse = new RabbitResponse<object>
                {
                    Data = response,
                    MessageId = Guid.NewGuid()
                };

                if (replyTo is not null)
                {
                    var corr = correlationId is null ? Guid.Empty : Guid.Parse(correlationId);
                    logger.LogInformation("Replying to message corr={corr} on {replyTo} queue", corr.ToString(), replyTo);
                    await rabbitMqService.PublishMessage(replyTo, corr, rabbitResponse, _channel, cancellationToken);
                    await rabbitMqService.PublishMessage("DEBUG_RESPONSES", corr, rabbitResponse, _channel, cancellationToken);
                }

                await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);

                logger.LogInformation("Message corr={CorrelationId} acknowledged.", correlationId);
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

