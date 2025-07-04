using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ExitCommandHandler : ICommandProcessor
    {
        public Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            ConsoleOutputHelper.WriteInfo("Exiting GroqSharp. Goodbye!");
            context.ShouldExit = true;
            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/exit" };
    }
}
