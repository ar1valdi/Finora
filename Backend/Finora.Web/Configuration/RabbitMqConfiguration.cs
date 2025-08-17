namespace Finora.Web.Configuration
{
    public class RabbitMqConfiguration
    {
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string VirtualHost { get; set; } = string.Empty;
    }
}
