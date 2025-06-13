using GroqSharp.Core;
using GroqSharp.Services;

namespace GroqSharp.Commands.Models
{
    public class CommandContext
    {
        private readonly ConversationPersistenceService _persistence;
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public IGroqService GroqService { get; set; }
        public ConversationService Conversation { get; set; } = new();
        public string CurrentModel { get; set; }
        public Dictionary<string, object> State { get; } = new();
        public object PreviousCommandResult { get; set; }
        public bool ShouldExit { get; set; }
        public string? LoadedArchiveFileName { get; set; }

        public CommandContext(ConversationPersistenceService persistence)
        {
            _persistence = persistence;
            Conversation = new ConversationService();
            var loadedMessages = persistence.LoadConversation(SessionId);
            if (loadedMessages != null)
            {
                Conversation.LoadMessages(loadedMessages);
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

        public void SaveConversation()
        {
            var messages = Conversation.GetHistory().ToList();

            if (!string.IsNullOrWhiteSpace(LoadedArchiveFileName))
            {
                _persistence.SaveConversation(LoadedArchiveFileName, messages);
            }
            else
            {
                _persistence.SaveConversation(messages); // fallback to new archive
            }
        }
    }
}
