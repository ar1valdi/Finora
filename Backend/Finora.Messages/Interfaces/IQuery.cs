namespace Finora.Messages.Interfaces
{
    public abstract class IQuery : IMessageWithCorrelationId
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
