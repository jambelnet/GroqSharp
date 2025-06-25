using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.Core.Services
{
    /// <summary>
    /// Provides higher-level orchestration for Groq chat and model operations.
    /// </summary>
    public class GroqService : IGroqService
    {
        private readonly IGroqClient _groqClient;

        public GroqService(IGroqClient groqClient)
        {
            _groqClient = groqClient ?? throw new ArgumentNullException(nameof(groqClient));
        }

        public async Task<string> GetChatCompletionAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

            return await _groqClient.CompleteChatAsync(prompt);
        }

        public async Task<string> GetChatCompletionAsync(ChatRequest request)
        {
            return await _groqClient.CompleteChatAsync(request);
        }

        public async Task<ChatCompletionResponse> GetStructuredResponseAsync(ChatRequest request)
        {
            return await _groqClient.CompleteStructuredChatAsync(request);
        }

        public IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request)
        {
            return _groqClient.StreamChatCompletionAsync(request);
        }

        public Task<List<string>> GetAvailableModelsAsync()
        {
            return _groqClient.GetAvailableModelsAsync();
        }

        public Task<string> GetDefaultModelAsync()
        {
            return _groqClient.GetDefaultModelAsync();
        }
    }
}
