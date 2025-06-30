using GroqSharp.Core.Models;
using GroqSharp.Core.Services;

namespace GroqSharp.Core.Interfaces
{
    public interface IGlobalConversationService
    {
        Task<ConversationSession> GetOrCreateSessionAsync(string sessionId);
        Task SaveSessionAsync(string sessionId, ConversationService conversation);
        Task<List<ConversationMeta>> ListAllConversationsAsync();
        Task<bool> RenameConversationAsync(string sessionId, string newTitle);
        Task<bool> DeleteConversationAsync(string sessionId);
    }
}