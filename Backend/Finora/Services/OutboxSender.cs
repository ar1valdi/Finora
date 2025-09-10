using Finora.Repositories.Interfaces;
using Finora.Web.Services;
using Microsoft.Extensions.Logging;

namespace Finora.Services;

public interface IOutboxSender {
    Task Run(int interval, CancellationToken cancellationToken);
}

public class OutboxSender(
    IOutboxMessageRepository outboxMessageRepository,
    IRabbitMqService rabbitMqService,
    ILogger<OutboxSender> logger,
    ICircuitBreakerStateHolder circuitBreakerStateHolder,
    IDbHealthCheck dbHealthCheck) : IOutboxSender {
    public async Task Run(int interval, CancellationToken cancellationToken)
    {
        var channel = await rabbitMqService.GetChannel(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var dbHealthy = circuitBreakerStateHolder.Get() == CircuitState.Closed;
                if (!dbHealthy)
                {
                    await Task.Delay(10 * interval, cancellationToken);
                    await dbHealthCheck.IsHealthyAsync(cancellationToken);
                    continue;
                }

                var pendingMessages = await outboxMessageRepository.GetPendingMessagesAsync(cancellationToken);

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        await rabbitMqService.PublishFromJson(
                            message.Exchange,
                            message.RoutingKey,
                            message.CorrelationId,
                            Guid.NewGuid(),
                            message.Response,
                            channel,
                            cancellationToken
                        ); 
                        await rabbitMqService.PublishFromJson(
                            message.Exchange,
                            "DEBUG_RESPONSES",
                            message.CorrelationId,
                            Guid.NewGuid(),
                            message.Response,
                            channel,
                            cancellationToken
                        );
                        message.Status = Kernel.OutboxMessageStatus.Sent;
                        logger.LogInformation("Sent outbox message {MessageId} to queue {QueueName}", message.Id, message.RoutingKey);
                    }
                    catch (Exception ex)
                    {
                        message.Status = Kernel.OutboxMessageStatus.Failed;
                        var stillHealthy = circuitBreakerStateHolder.Get() == CircuitState.Closed;
                        if (stillHealthy)
                        {
                            logger.LogError(ex, "Error sending outbox message {MessageId}. Retrying in next run.", message.Id);
                        }
                    }
                }

                await outboxMessageRepository.UpdateRangeAsync(pendingMessages, cancellationToken);

                await Task.Delay(interval, cancellationToken);
            }
            catch (Exception ex)
            {
                var dbHealthy = circuitBreakerStateHolder.Get() == CircuitState.Closed;
                if (dbHealthy)
                {
                    logger.LogError(ex, "Error in OutboxSender. Retrying in next run.");
                }
            }
        }
    }
}