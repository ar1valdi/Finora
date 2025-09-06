namespace Finora.Messages.Interfaces
{
    public abstract class IMessage
    {
        public Guid MessageId { get; set; } = Guid.Empty;
    }
}
