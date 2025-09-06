using Finora.Messages.Interfaces;

namespace Finora.Messages.Wrappers
{
    public class RabbitResponse<T> : IMessage
    {
        public Guid MessageId { get; set; } = Guid.Empty;
        public T? Data { get; set; } = default;
    }
}
