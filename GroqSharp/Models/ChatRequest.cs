using System.Text.Json.Serialization;

namespace GroqSharp.Models
{
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "llama-3.3-70b-versatile";

        [JsonPropertyName("messages")]
        public Message[] Messages { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; } = 1024;

        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 1;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("stop")]
        public string[] Stop { get; set; } = null;

        [JsonPropertyName("frequency_penalty")]
        public double FrequencyPenalty { get; set; } = 0;

        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; } = 0;
    }
}
