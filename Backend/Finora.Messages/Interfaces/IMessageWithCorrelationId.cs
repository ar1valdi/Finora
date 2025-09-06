using MediatR;

namespace Finora.Messages.Interfaces
{
    public abstract class IMessage : IRequest<object>
    {
        public Guid MessageId { get; set; } = Guid.Empty;
    }
}
