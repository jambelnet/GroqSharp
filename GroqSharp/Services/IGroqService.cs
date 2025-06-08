using GroqSharp.Models;

namespace GroqSharp.Services
{
    public interface IGroqService
    {
        /// <summary>
        /// Gets a chat completion response for a simple prompt string.
        /// </summary>
        /// <param name="prompt">The prompt text to send to the model.</param>
        /// <returns>The model's response as a string.</returns>
        Task<string> GetChatCompletionAsync(string prompt);

        /// <summary>
        /// Gets a chat completion response using a detailed chat request.
        /// </summary>
        /// <param name="request">The ChatRequest containing model parameters and messages.</param>
        /// <returns>The model's response as a string.</returns>
        Task<string> GetChatCompletionAsync(ChatRequest request);

        /// <summary>
        /// Retrieves a list of available models from the backend.
        /// </summary>
        /// <returns>A list of model names.</returns>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// Streams chat completions from the model in real-time
        /// </summary>
        IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request);
    }
}
