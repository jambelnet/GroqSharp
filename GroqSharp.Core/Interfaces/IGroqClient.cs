using GroqSharp.Core.Models;

namespace GroqSharp.Core.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the Groq chat API via HTTP.
    /// </summary>
    public interface IGroqClient
    {
        /// <summary>
        /// Sends a user prompt and returns the model's raw response.
        /// </summary>
        Task<string> CompleteChatAsync(string userMessage);

        /// <summary>
        /// Sends a structured chat request to the model and returns the raw response.
        /// </summary>
        Task<string> CompleteChatAsync(ChatRequest request);

        /// <summary>
        /// Sends a structured request and returns a parsed chat response (for reasoning, tools, etc.).
        /// </summary>
        Task<ChatCompletionResponse> CompleteStructuredChatAsync(ChatRequest request);

        /// <summary>
        /// Streams chat completion tokens as they arrive.
        /// </summary>
        IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request);

        /// <summary>
        /// Returns a list of available models.
        /// </summary>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// Gets the default model configured.
        /// </summary>
        Task<string> GetDefaultModelAsync();
    }
}
