﻿using System.Text.Json.Serialization;

namespace GroqSharp.Core.Models
{
    public record ChatRequest
    {
        /// <summary>
        /// ID of the model to use (default: llama-3.3-70b-versatile)
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "llama-3.3-70b-versatile";

        /// <summary>
        /// Conversation messages
        /// </summary>
        [JsonPropertyName("messages")]
        public Message[]? Messages { get; set; }

        /// <summary>
        /// Sampling temperature (default: 0.7)
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// Maximum number of tokens to generate (default: 1024)
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; } = 1024;

        /// <summary>
        /// Nucleus sampling: probability mass to consider (default: 1)
        /// </summary>
        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 1.0;

        /// <summary>
        /// Whether to stream partial responses (default: false)
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        /// <summary>
        /// Stop sequences (optional)
        /// </summary>
        [JsonPropertyName("stop")]
        public string[]? Stop { get; set; }

        /// <summary>
        /// Penalizes new tokens based on their existing frequency (default: 0)
        /// </summary>
        [JsonPropertyName("frequency_penalty")]
        public double FrequencyPenalty { get; set; } = 0.0;

        /// <summary>
        /// Penalizes new tokens based on whether they appear in the text so far (default: 0)
        /// </summary>
        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; } = 0.0;

        /// <summary>
        /// Controls how model reasoning is presented ("raw", "parsed", or "hidden").
        /// </summary>
        [JsonPropertyName("reasoning_format")]
        public string? ReasoningFormat { get; set; }

        /// <summary>
        /// Controls how much effort the model spends on reasoning ("default" or "none", only for Qwen models).
        /// </summary>
        [JsonPropertyName("reasoning_effort")]
        public string? ReasoningEffort { get; set; }

        /// <summary>
        /// Settings to customize web search behavior, such as including or excluding specific domains
        /// and boosting results from a particular country. Applies when using agentic tooling models.
        /// </summary>
        [JsonPropertyName("search_settings")]
        public SearchSettings? SearchSettings { get; set; }
    }
}
