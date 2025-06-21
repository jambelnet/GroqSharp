using System.Text.Json.Serialization;

namespace GroqSharp.Core.Models
{
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public object Content { get; set; }

        // Remove the Timestamp property for API requests
        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
