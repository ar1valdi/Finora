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
    ILogger<OutboxSender> logger) : IOutboxSender {
    public async Task Run(int interval, CancellationToken cancellationToken)
    {
        var channel = await rabbitMqService.GetChannel(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {

                var pendingMessages = await outboxMessageRepository.GetPendingMessagesAsync(cancellationToken);

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        //if (await QueueExists(message.ReplyTo))
                        {
                            await rabbitMqService.PublishFromJson(
                                string.Empty,
                                message.ReplyTo,
                                message.CorrelationId,
                                Guid.NewGuid(),
                                message.Response,
                                channel,
                                cancellationToken
                            ); 
                            await rabbitMqService.PublishFromJson(
                                string.Empty,
                                "DEBUG_RESPONSES",
                                message.CorrelationId,
                                Guid.NewGuid(),
                                message.Response,
                                channel,
                                cancellationToken
                            );
                            message.Status = Kernel.OutboxMessageStatus.Sent;
                            logger.LogInformation("Sent outbox message {MessageId} to queue {QueueName}", message.Id, message.ReplyTo);
                        }
                        //else
                        //{
                        //    message.Status = Kernel.OutboxMessageStatus.Failed;
                        //}
                    }
                    catch (Exception ex)
                    {
                        message.Status = Kernel.OutboxMessageStatus.Failed;
                        logger.LogError("Error on sending resposne: {ex}. Retrying in next run.", ex);
                    }
                }

                await outboxMessageRepository.UpdateRangeAsync(pendingMessages, cancellationToken);

                await Task.Delay(interval, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in OutboxSender: {ex}. Retrying in next run.", ex);
            }
        }
    }

    private async Task<bool> QueueExists(string queue)
    {
        var channel = rabbitMqService.GetChannel(CancellationToken.None).Result;

        try
        {
            await channel.QueueDeclarePassiveAsync(queue);
            await channel.DisposeAsync();
            return true;
        }
        catch
        {
            await channel.DisposeAsync();
            return false;
        }
    }
}