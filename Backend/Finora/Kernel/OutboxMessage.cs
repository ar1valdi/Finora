namespace Finora.Kernel;

public class OutboxMessage : EntityBase
{
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; } = Guid.Empty;
    public string Response { get; set; } = string.Empty;
    public ulong DeliveryTag { get; set; } = 0;
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
}

public enum OutboxMessageStatus
{
    Pending,
    Sent,
    Failed
}


