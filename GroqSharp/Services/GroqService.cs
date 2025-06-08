using GroqSharp.Models;

namespace GroqSharp.Services
{
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
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _groqClient.CompleteChatAsync(request);
        }

        public IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return _groqClient.StreamChatCompletionAsync(request);
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            return await _groqClient.GetAvailableModelsAsync();
        }
    }
}
