using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Routing
{
    public class CommandRouter : ICommandRouter
    {
        private readonly IEnumerable<ICommandProcessor> _handlers;

        public CommandRouter(IEnumerable<ICommandProcessor> handlers)
        {
            _handlers = handlers;
        }

        public async Task<bool> RouteCommand(string input, CliSessionContext context)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0];
            var args = parts.Skip(1).ToArray();

            foreach (var handler in _handlers)
            {
                if (await handler.ProcessCommand(command, args, context))
                    return true;
            }

            return false;
        }

        public async Task<string?> RunCommandAsync(string commandLine, CliSessionContext context)
        {
            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0];
            var args = parts.Skip(1).ToArray();

            foreach (var handler in _handlers)
            {
                if (await handler.ProcessCommand(command, args, context))
                {
                    if (context.PreviousCommandResult is string resultText)
                        return resultText;
                }
            }

            return null;
        }
    }
}
