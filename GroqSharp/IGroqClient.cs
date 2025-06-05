using GroqSharp.Models;

namespace GroqSharp
{
    public interface IGroqClient
    {
        Task<string> CompleteChatAsync(ChatRequest request);
        Task<string> CompleteChatAsync(string userMessage);
        Task<List<string>> GetAvailableModelsAsync();
    }
}
