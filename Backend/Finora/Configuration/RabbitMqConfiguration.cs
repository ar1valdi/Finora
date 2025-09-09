namespace Finora.Web.Configuration
{
    public class RabbitMqConfiguration
    {
        // Connection
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string VirtualHost { get; set; } = string.Empty;
        
        // Requests
        public string RequestExchange { get; set; } = string.Empty;
        public string RequestQueue { get; set; } = string.Empty;
        public string QueryQueue { get; set; } = string.Empty;
        public string CommandRoutingKey { get; set; } = string.Empty;
        public string QueryRoutingKey { get; set; } = string.Empty;
        
        // Internal
        public string InternalExchangeName { get; set; } = string.Empty;
        public string MailingRoutingKey { get; set; } = string.Empty;
    }
}
