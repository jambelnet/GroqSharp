using System.Text.Json.Serialization;

namespace GroqSharp.Core.Models
{
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public object Content { get; set; }

        [JsonPropertyName("executed_tools")]
        public List<ExecutedTool>? ExecutedTools { get; set; }
    }
}
