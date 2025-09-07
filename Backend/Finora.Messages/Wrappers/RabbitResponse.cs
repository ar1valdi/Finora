using Finora.Messages.Interfaces;

namespace Finora.Messages.Wrappers
{
    public class RabbitResponse<T> : IMessage
    {
        public T? Data { get; set; } = default;
        public int StatusCode { get; set; } = 200;
        public string[] Errors { get; set; } = new string[0];
    }
}
