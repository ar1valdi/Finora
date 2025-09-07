using Finora.Messages.Wrappers;
using MediatR;

namespace Finora.Messages.Interfaces
{
    public abstract class IMessage : IRequest<RabbitResponse<object>>
    {
        public Guid MessageId { get; set; } = Guid.Empty;
    }
}
