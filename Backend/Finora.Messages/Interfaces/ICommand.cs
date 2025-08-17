namespace Finora.Messages.Interfaces
{
    public abstract class ICommand : IMessageWithCorrelationId
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
