using GroqSharp.Commands.Interfaces;
using GroqSharp.Commands.Models;

namespace GroqSharp.Commands.Handlers
{
    public class HistoryCommandHandler : ICommandProcessor
    {
        public Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/history", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            Console.WriteLine("\nConversation History:");
            foreach (var msg in context.Conversation.GetFullHistory())
            {
                Console.ForegroundColor = msg.Role == "user" ? ConsoleColor.Cyan : ConsoleColor.Green;
                Console.WriteLine($"[{msg.Role}] {msg.Content}");
                Console.ResetColor();
            }
            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/history" };
    }
}
