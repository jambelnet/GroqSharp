using GroqSharp.Models;

namespace GroqSharp.Services
{
    public class GroqService : IGroqService
    {
        private readonly IGroqClient _groqClient;

        public GroqService(IGroqClient groqClient)
        {
            _groqClient = groqClient;
        }

        public async Task<string> GetChatCompletionAsync(string prompt)
        {
            return await _groqClient.CompleteChatAsync(prompt);
        }

        public async Task<string> GetChatCompletionAsync(ChatRequest request)
        {
            return await _groqClient.CompleteChatAsync(request);
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            return await _groqClient.GetAvailableModelsAsync();
        }
    }
}
