using System.Text.Json.Serialization;

namespace GroqSharp.Models
{
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
