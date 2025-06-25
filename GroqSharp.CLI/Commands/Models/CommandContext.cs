using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Services;

namespace GroqSharp.CLI.Commands.Models
{
    public class CommandContext
    {
        private readonly IGlobalConversationService _conversationService;
        public IGroqService GroqService { get; set; }
        public IServiceProvider ServiceProvider { get; }
        public ConversationService Conversation { get; private set; }
        public string CurrentModel { get; set; }
        public string CurrentTitle { get; private set; }
        public object PreviousCommandResult { get; set; }
        public string SessionId { get; private set; }
        public bool ShouldExit { get; set; }

        public CommandContext(IServiceProvider serviceProvider, IGlobalConversationService conversationService, ConversationService conversation)
        {
            ServiceProvider = serviceProvider;
            _conversationService = conversationService;
            Conversation = conversation;
        }

        public async Task InitializeSession(string sessionId, string title = null)
        {
            SessionId = sessionId;
            var session = await _conversationService.GetOrCreateSessionAsync(sessionId);
            Conversation = session.Conversation;
            CurrentTitle = title ?? session.Title;
        }

        public async Task SaveConversation()
        {
            if (Conversation != null && !string.IsNullOrEmpty(SessionId))
            {
                await _conversationService.SaveSessionAsync(SessionId);
            }
        }

        public async Task RenameConversation(string newTitle)
        {
            if (await _conversationService.RenameConversationAsync(SessionId, newTitle))
            {
                CurrentTitle = newTitle;
            }
        }

        // Helper methods
        public string Prompt(string message, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
            return Console.ReadLine();
        }

        public bool PromptYesNo(string message, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
            return Console.ReadLine()?.Equals("y", StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}
