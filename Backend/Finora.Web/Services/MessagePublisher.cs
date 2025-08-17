using Finora.Messages.Users;

namespace Finora.Web.Services
{
    public interface IMessagePublisher
    {
        Guid PublishUserCreation(AddUser user);
        Guid PublishCustomMessage<T>(string queueName, T message);
    }

    public class MessagePublisher : IMessagePublisher
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(IRabbitMqService rabbitMqService, ILogger<MessagePublisher> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public Guid PublishUserCreation(AddUser user)
        {
            _logger.LogInformation("Publishing user creation message for user: {Email}", user.Email);
            return _rabbitMqService.PublishMessage("user-creation", user);
        }

        public Guid PublishCustomMessage<T>(string queueName, T message)
        {
            _logger.LogInformation("Publishing custom message to queue: {QueueName}", queueName);
            return _rabbitMqService.PublishMessage(queueName, message);
        }
    }
}

