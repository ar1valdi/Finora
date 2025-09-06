using System.Text.Json.Serialization;

namespace Finora.Backend.Models.Concrete
{
    public enum MessageType
    {
        Command = 1,
        Query = 2
    }

    public class MessageEnvelope
    {
        [JsonPropertyName("messageType")]
        public MessageType MessageType { get; set; }
        
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;
        
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        public object Data { get; set; } = new object();

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; } = 200;

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();

        [JsonPropertyName("jwt")]
        public string? Jwt { get; set; } = string.Empty;
    }
}
