using GroqSharp.Core.Models;

namespace GroqSharp.Core.Interfaces
{
    /// <summary>
    /// High-level service abstraction for chat interactions and model operations.
    /// </summary>
    public interface IGroqService
    {
        /// <summary>
        /// Sends a simple user prompt and returns the model's response.
        /// </summary>
        Task<string> GetChatCompletionAsync(string prompt);

        /// <summary>
        /// Sends a full structured request and returns the model's raw response.
        /// </summary>
        Task<string> GetChatCompletionAsync(ChatRequest request);

        /// <summary>
        /// Sends a full structured request and returns the parsed model response.
        /// </summary>
        Task<ChatCompletionResponse> GetStructuredResponseAsync(ChatRequest request);

        /// <summary>
        /// Streams chat completions for real-time updates.
        /// </summary>
        IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request);

        /// <summary>
        /// Gets the list of available models.
        /// </summary>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// Gets the default model name.
        /// </summary>
        Task<string> GetDefaultModelAsync();
    }
}
