using GroqSharp.Core.Services.Models;

namespace GroqSharp.Core.Services.Interfaces
{
    public interface IGlobalConversationService
    {
        Task<ConversationSession> GetOrCreateSessionAsync(string sessionId);
        Task SaveSessionAsync(string sessionId);
        Task AutoSaveAllAsync();
        Task<List<ConversationMeta>> ListAllConversationsAsync();
        Task<bool> RenameConversationAsync(string sessionId, string newTitle);
        Task<bool> DeleteConversationAsync(string sessionId);
    }
}