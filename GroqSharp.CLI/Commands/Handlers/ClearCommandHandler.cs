using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ClearCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/clear", StringComparison.OrdinalIgnoreCase))
                return false;

            context.Conversation.ClearHistory();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Conversation history cleared.");
            Console.ResetColor();

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/clear" };
    }
}
