using GroqSharp.Commands.Interfaces;
using GroqSharp.Commands.Models;

namespace GroqSharp.Commands.Handlers
{
    public class NewCommandHandler : ICommandProcessor
    {
        public Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/new", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            if (context.Conversation.GetHistory().Any())
            {
                context.SaveConversation();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Previous chat saved.");
            }

            context.Conversation = new();
            context.LoadedArchiveFileName = null;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Started a new chat session.");
            Console.ResetColor();

            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/new" };
    }
}
