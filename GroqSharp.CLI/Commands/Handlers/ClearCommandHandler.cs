using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ClearCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/clear", StringComparison.OrdinalIgnoreCase))
                return false;

            context.Conversation.ClearHistory();
            ConsoleOutputHelper.WriteInfo("Conversation history cleared.");

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/clear" };
    }
}
