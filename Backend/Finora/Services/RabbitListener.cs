using System.Text;
using System.Text.Json;
using Finora.Backend.Models.Concrete;
using Finora.Backend.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Finora.Repositories.Interfaces;
using Finora.Kernel;

namespace Finora.Web.Services;

public interface IRabbitListener
{
    public Task Listen(string queueName, string? acceptingCqsType = null, CancellationToken cancellationToken = default);
}

public class RabbitListener(
    IRabbitMqService rabbitMqService,
    ILogger<RabbitListener> logger,
    IServiceProvider serviceProvider
) : IRabbitListener
{
    private IChannel? _channel;
    private const int MONITOR_REPLY_QUEUE_INTERVAL = 1000;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task Listen(string queueName, string? acceptingCqsType = null, CancellationToken cancellationToken = default)
    {
        _channel = await rabbitMqService.GetChannel(cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            using var messageScope = serviceProvider.CreateScope();
            var mediator = messageScope.ServiceProvider.GetRequiredService<IMediator>();
            var unitOfWork = messageScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

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

                if (acceptingCqsType is not null && cqsType != acceptingCqsType)
                {
                    throw new InvalidOperationException($"Message type {cqsType} is not accepted by this listener");
                }

                logger.LogInformation("Message details:\n" +
                    "Correlation Id: {CorrelationId}\n" +
                    "Reply To: {ReplyTo}\n" +
                    "Message Id: {MessageId}\n" +
                    "Message Type: {MessageType}\n" +
                    "CQS Type: {cqsType}\n" +
                    "Body: {Body}", correlationId, replyTo, messageId, msgType, cqsType, envelope.Data);

                var request = MessageMapper.DeserializeMessage(msgType, envelope.Data.ToString() ?? "{}");

                var handlerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var responseMonitoringCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var responseQueueMonitoring = MonitorReplyQueue(replyTo, handlerCts, responseMonitoringCts.Token);
                
                if (handlerCts.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Response queue does not exist.");
                }

                using var transaction = await unitOfWork.BeginTransactionAsync(handlerCts.Token);

                var response = await mediator.Send(request, handlerCts.Token);
                OutboxMessage? outboxMsg = null;

                if (replyTo is not null && !handlerCts.IsCancellationRequested)
                {
                    var corr = string.IsNullOrEmpty(correlationId) ? Guid.Empty : Guid.Parse(correlationId);
                    outboxMsg = new OutboxMessage
                    {
                        ReplyTo = replyTo,
                        CorrelationId = corr,
                        Response = response,
                        DeliveryTag = args.DeliveryTag
                    };                
                }
                
                await unitOfWork.CommitAsync(outboxMsg, handlerCts.Token);
                await responseMonitoringCts.CancelAsync();

                if (replyTo is not null && !handlerCts.IsCancellationRequested)
                {
                    var corr = correlationId is null ? Guid.Empty : Guid.Parse(correlationId);
                    logger.LogInformation("Replying to message corr={corr} on {replyTo} queue", corr.ToString(), replyTo);
                    await rabbitMqService.PublishMessage(replyTo, corr, response, _channel, cancellationToken);
                    await rabbitMqService.PublishMessage("DEBUG_RESPONSES", corr, response, _channel, cancellationToken);
                }

                await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);

                logger.LogInformation("Message corr={CorrelationId} written to outbox.", correlationId);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Operation cancelled - timeout or app closed.");
                await _channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
            }
            catch (Exception ex)
            {
                await _channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
                logger.LogError(ex, "Failed to process message from queue {QueueName}. DeliveryTag: {DeliveryTag}", queueName, args.DeliveryTag);
            }
        };

        logger.LogInformation("Listening to {QueueName}...", queueName);
        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);
    }

    private async Task MonitorReplyQueue(string? replyTo, CancellationTokenSource cts, CancellationToken ct)
    {
        if (replyTo is null)
        {
            return;
        }

        var channel = await rabbitMqService.GetChannel(cts.Token);

        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(MONITOR_REPLY_QUEUE_INTERVAL, ct);
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try {
                await channel.QueueDeclarePassiveAsync(replyTo, ct);
            }
            catch {
                logger.LogWarning("Response queue {ReplyTo} does not exist.", replyTo);
                await cts.CancelAsync();
                break;
            }
        }
    }
}

