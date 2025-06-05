using System.Text.Json.Serialization;

namespace GroqSharp.Models
{
    public class ChatResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string? SystemFingerprint { get; set; }

        [JsonPropertyName("x_groq")]
        public XGroq? XGroq { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("logprobs")]
        public object? LogProbs { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("queue_time")]
        public double QueueTime { get; set; }

        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("prompt_time")]
        public double PromptTime { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("completion_time")]
        public double CompletionTime { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("total_time")]
        public double TotalTime { get; set; }
    }

    public class XGroq
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
