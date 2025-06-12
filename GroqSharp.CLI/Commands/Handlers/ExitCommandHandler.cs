using GroqSharp.Commands.Interfaces;
using GroqSharp.Commands.Models;

namespace GroqSharp.Commands.Handlers
{
    public class ExitCommandHandler : ICommandProcessor
    {
        public Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            context.ShouldExit = true;
            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/exit" };
    }
}
