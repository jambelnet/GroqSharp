using GroqSharp.Core.Enums;
using System.Text.Json.Serialization;

namespace GroqSharp.Core.Models
{
    public class Message
    {
        [JsonPropertyName("role")]
        public MessageRole Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("executed_tools")]
        public List<ExecutedTool>? ExecutedTools { get; set; }
    }
}
