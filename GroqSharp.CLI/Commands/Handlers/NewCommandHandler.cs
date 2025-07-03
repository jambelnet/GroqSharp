using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class NewCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/new", StringComparison.OrdinalIgnoreCase))
                return false;

            if (context.Conversation.GetFullHistory().Any())
            {
                await context.SaveConversation();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Previous chat saved.");
                Console.ResetColor();
            }

            var title = args.Length > 0 ? string.Join(" ", args) : null;
            await context.InitializeAsync(Guid.NewGuid().ToString(), title);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Started new chat session: {context.Title}");
            Console.ResetColor();

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/new" };
    }
}
