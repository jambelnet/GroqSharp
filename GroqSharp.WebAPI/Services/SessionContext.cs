using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using GroqSharp.Core.Services;

namespace GroqSharp.WebAPI.Services
{
    public class SessionContext
    {
        public string SessionId { get; }
        public ConversationSession Session { get; }
        public ConversationService Conversation { get; }

        private SessionContext(string sessionId, ConversationSession session, ConversationService conversation)
        {
            SessionId = sessionId;
            Session = session;
            Conversation = conversation;
        }

        public static async Task<SessionContext> CreateAsync(string sessionId, IGlobalConversationService globalService)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            var session = await globalService.GetOrCreateSessionAsync(sessionId);
            var conversation = new ConversationService(globalService);
            conversation.LoadFromSession(session);
            return new SessionContext(sessionId, session, conversation);
        }

    }
}
