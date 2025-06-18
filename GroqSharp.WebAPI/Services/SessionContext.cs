using GroqSharp.Core.Services;
using GroqSharp.Core.Services.Interfaces;
using GroqSharp.Core.Services.Models;

namespace GroqSharp.WebAPI.Services
{
    public class SessionContext
    {
        public string SessionId { get; }
        public ConversationSession Session { get; }
        public ConversationService Conversation => Session.Conversation;

        public SessionContext(string sessionId, ConversationSession session)
        {
            SessionId = sessionId;
            Session = session;
        }


        public static async Task<SessionContext> CreateAsync(string sessionId, IGlobalConversationService globalService)
        {
            var session = await globalService.GetOrCreateSessionAsync(sessionId);
            return new SessionContext(sessionId, session);
        }
    }
}
