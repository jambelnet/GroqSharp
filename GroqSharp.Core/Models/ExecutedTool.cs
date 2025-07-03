using System.Text.Json.Serialization;

namespace GroqSharp.Core.Models
{
    public class ExecutedTool
    {
        [JsonPropertyName("tool_name")]
        public string ToolName { get; set; }

        [JsonPropertyName("input")]
        public string Input { get; set; }

        [JsonPropertyName("output")]
        public string Output { get; set; }

        [JsonPropertyName("score")]
        public float? Score { get; set; }
    }
}
