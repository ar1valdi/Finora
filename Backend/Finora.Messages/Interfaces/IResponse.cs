
namespace Finora.Messages.Interfaces
{
    public abstract class IResponse : IMessageWithCorrelationId
    {
        public Guid CorrelationId { get; set; }
    }
}
