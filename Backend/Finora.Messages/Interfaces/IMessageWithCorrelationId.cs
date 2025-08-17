namespace Finora.Messages.Interfaces
{
    public interface IMessageWithCorrelationId
    {
        public Guid CorrelationId { get; set; }
    }
}
