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
        
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        public object Data { get; set; } = new object();
        
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
    }
}
