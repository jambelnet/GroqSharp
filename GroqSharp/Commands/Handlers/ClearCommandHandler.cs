using GroqSharp.Commands.Interfaces;
using GroqSharp.Commands.Models;

namespace GroqSharp.Commands.Handlers
{
    public class ClearCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/clear", StringComparison.OrdinalIgnoreCase))
                return false;

            context.Conversation.ClearHistory();
            Console.WriteLine("Conversation history cleared.");
            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/clear" };
    }
}
