using GroqSharp.Models;

namespace GroqSharp.Core
{
    /// <summary>
    /// Defines methods for interacting with the Groq chat API.
    /// </summary>
    public interface IGroqClient
    {
        /// <summary>
        /// Sends a structured chat request to the model and returns the raw response.
        /// </summary>
        /// <param name="request">The full chat request including model, messages, and settings.</param>
        /// <returns>The model's response as a string.</returns>
        Task<string> CompleteChatAsync(ChatRequest request);

        /// <summary>
        /// Sends a basic user message to the model and returns the raw response.
        /// </summary>
        /// <param name="userMessage">The user's message string.</param>
        /// <returns>The model's response as a string.</returns>
        Task<string> CompleteChatAsync(string userMessage);

        /// <summary>
        /// Retrieves the list of available language models.
        /// </summary>
        /// <returns>A list of available model identifiers.</returns>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// Retrieves the default language model.
        /// </summary>
        /// <returns>The default model identifier.</returns>
        Task<string> GetDefaultModelAsync();

        /// <summary>
        /// Streams chat completions from the model in real-time
        /// </summary>
        IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request);
    }
}
