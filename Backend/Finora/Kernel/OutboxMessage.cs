namespace Finora.Kernel;

public class OutboxMessage : EntityBase
{
    public string ReplyTo { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; } = Guid.Empty;
    public object Response { get; set; } = new object();
    public ulong DeliveryTag { get; set; } = 0;
}

