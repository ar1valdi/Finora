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
using Finora.Backend.Common;
using Finora.Services;
using System.ComponentModel.DataAnnotations;
using Finora.Messages.Wrappers;

namespace Finora.Web.Services;

public interface IRabbitListener
{
    public Task Listen(string queueName, string? acceptingCqsType = null, CancellationToken cancellationToken = default);
}

public class RabbitListener(
    IRabbitMqService rabbitMqService,
    ILogger<RabbitListener> logger,
    IServiceProvider serviceProvider,
    IOutboxMessageRepository outboxRepo,
    IDbHealthCheck dbHealthCheck
) : IRabbitListener
{
    private IChannel? _channel;
    private const int MONITOR_REPLY_QUEUE_INTERVAL = 1000;
    
    public async Task Listen(string queueName, string? acceptingCqsType = null, CancellationToken cancellationToken = default)
    {
        _channel = await rabbitMqService.GetChannel(cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);

        var shouldWrapInTransaction = acceptingCqsType == "Command";

        string? replyTo = null;
        Guid corr = Guid.Empty;

        consumer.ReceivedAsync += async (sender, args) =>
        {
            if (!await dbHealthCheck.IsHealthyAsync(cancellationToken))
            {
                logger.LogError("Database is not healthy. Skipping message processing.");
                await _channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
                return;
            }

            using var messageScope = serviceProvider.CreateScope();
            var mediator = messageScope.ServiceProvider.GetRequiredService<IMediator>();
            var unitOfWork = messageScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                logger.LogInformation("Received message from queue {QueueName}", queueName);

                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var envelope = JsonSerializer.Deserialize<MessageEnvelope>(message, JsonConfig.JsonOptions);

                if (envelope is null)
                {
                    throw new InvalidDataException($"Failed to deserialize message from queue {queueName}");
                }

                var messageId = envelope.MessageId;
                var correlationId = args.BasicProperties.CorrelationId;
                replyTo = args.BasicProperties.ReplyTo;
                var msgType = envelope.Type;
                var cqsType = envelope.MessageType.ToString();

                if (acceptingCqsType is not null && cqsType != acceptingCqsType)
                {
                    throw new InvalidOperationException($"Message type {cqsType} is not accepted by this listener");
                }

                corr = string.IsNullOrEmpty(correlationId) ? Guid.Empty : Guid.Parse(correlationId);
                if (corr != Guid.Empty && await outboxRepo.IsInOutbox(corr, cancellationToken))
                {
                    logger.LogInformation("Message with correlation id {CorrelationId} already processed. Acknowledging and skipping.", correlationId);
                    await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
                    return;
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

                if (shouldWrapInTransaction)
                {
                    await unitOfWork.BeginTransactionAsync(handlerCts.Token);
                }

                var response = await mediator.Send(request, handlerCts.Token);
                OutboxMessage? outboxMsg = null;

                if (replyTo is not null && !handlerCts.IsCancellationRequested)
                {
                    outboxMsg = new OutboxMessage
                    {
                        RoutingKey = replyTo,
                        CorrelationId = corr,
                        Response = JsonSerializer.Serialize(response, JsonConfig.JsonOptions),
                        DeliveryTag = args.DeliveryTag
                    };
                    await outboxRepo.AddAsync(outboxMsg, handlerCts.Token);
                }
                
                unitOfWork.JobHandled();
                if (shouldWrapInTransaction)
                {
                    await unitOfWork.CommitAsync(handlerCts.Token);
                }
                await responseMonitoringCts.CancelAsync();

                await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);

                logger.LogInformation("Message corr={CorrelationId} written to outbox.", correlationId);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Operation cancelled - timeout or app closed.");
                if (shouldWrapInTransaction)
                {
                    await unitOfWork.RollbackAsync();
                }
                await _channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
            }
            catch (Exception ex)
            {
                await _channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
                if (shouldWrapInTransaction)
                {
                    await unitOfWork.RollbackAsync();
                }
                logger.LogError(ex, "Failed to process message from queue {QueueName}. DeliveryTag: {DeliveryTag}", queueName, args.DeliveryTag);
            
                try {
                    if (replyTo is not null)
                    {
                        var response = GetErrorRabbitMessageFromException(ex, replyTo, corr);
                        await outboxRepo.AddAsync(new OutboxMessage
                        {
                            RoutingKey = replyTo,
                            CorrelationId = corr,
                            Response = JsonSerializer.Serialize(response, JsonConfig.JsonOptions),
                            DeliveryTag = args.DeliveryTag
                        }, cancellationToken);
                    }
                } catch {
                    logger.LogError("Failed to add error message to outbox. DeliveryTag: {DeliveryTag}", args.DeliveryTag);
                }
            }
        };

        logger.LogInformation("Listening to {QueueName}...", queueName);
        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);
    }

    private RabbitResponse<object> GetErrorRabbitMessageFromException(Exception ex, string replyTo, Guid correlationId)
    {
        var statusCode = ex.Message.Contains("Validation", StringComparison.OrdinalIgnoreCase) == true ? 400 : 500;

        var message = ex.Message;

        var response = new RabbitResponse<object>
        {
            Data = null,
            StatusCode = statusCode,
            Errors = [message]
        };
        
        return response;
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
            try
            {
                await Task.Delay(MONITOR_REPLY_QUEUE_INTERVAL, ct);
            }
            catch (TaskCanceledException)
            {
                break;
            }

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

        await channel.DisposeAsync();
    }

    ~RabbitListener()
    {
        if (_channel is not null)
        {
            _channel.Dispose();
        }
    }
}

