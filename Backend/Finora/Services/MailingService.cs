using System.Text.Json;
using Finora.Backend.Common;
using Finora.Backend.Models.Concrete;
using Finora.Kernel;
using Finora.Messages.Mail;
using Finora.Repositories.Interfaces;
using Finora.Web.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finora.Services;

public interface IMailingService {
    void SendEmailAtJobHandled(string to, string subject, string body, Guid? correlationId = null, CancellationToken cancellationToken = default);
}

public class MailingService : IMailingService {
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MailingService> _logger;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private List<OutboxMessage> _pendingMessages = new List<OutboxMessage>();

    public MailingService(
        IOptions<RabbitMqConfiguration> configuration,
        IUnitOfWork unitOfWork,
        ILogger<MailingService> logger,
        IOutboxMessageRepository outboxMessageRepository
    ) {
        _exchangeName = configuration.Value.InternalExchangeName;
        _routingKey = configuration.Value.MailingRoutingKey;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _outboxMessageRepository = outboxMessageRepository;

        unitOfWork.OnJobHandled += SendPendingMessages;
    }

    private void SendPendingMessages(object? sender, EventArgs e) {
        _outboxMessageRepository.AddRangeAsync(_pendingMessages, CancellationToken.None).Wait();
    }

    public void SendEmailAtJobHandled(
        string to, 
        string subject, 
        string body, 
        Guid? correlationId = null, 
        CancellationToken cancellationToken=default
    ) {
        _logger.LogInformation("Adding email to pending messages: to={To}, subject={Subject}", to, subject);
        
        var mailMessage = new MailRequest{
            To = to,
            Subject = subject,
            Body = body
        };

        var envelope = new MessageEnvelope
        {
            MessageId = Guid.NewGuid().ToString(),
            Type = "MailRequest",
            MessageType = MessageType.Query,
            Data = mailMessage,
        };

        var outboxMessage = new OutboxMessage{
            Exchange = _exchangeName,
            RoutingKey = _routingKey,
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Response = JsonSerializer.Serialize(envelope, JsonConfig.JsonOptions),
            DeliveryTag = 0,
            Status = OutboxMessageStatus.Pending
        };
        
        _pendingMessages.Add(outboxMessage);
    }
}