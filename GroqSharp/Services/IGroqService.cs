using GroqSharp.Models;

namespace GroqSharp.Services
{
    public interface IGroqService
    {
        Task<string> GetChatCompletionAsync(string prompt);
        Task<string> GetChatCompletionAsync(ChatRequest request);
        Task<List<string>> GetAvailableModelsAsync();
    }
}
