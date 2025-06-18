using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class NewCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/new", StringComparison.OrdinalIgnoreCase))
                return false;

            if (context.Conversation.GetHistory().Any())
            {
                await context.SaveConversation();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Previous chat saved.");
            }

            var title = args.Length > 0 ? string.Join(" ", args) : null;
            await context.InitializeSession(Guid.NewGuid().ToString(), title);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Started new chat session: {context.CurrentTitle}");
            Console.ResetColor();

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/new" };
    }
}
