using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;

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
                ConsoleOutputHelper.WriteInfo("Previous chat saved.");
            }

            var title = args.Length > 0 ? string.Join(" ", args) : null;
            await context.InitializeAsync(Guid.NewGuid().ToString(), title);

            ConsoleOutputHelper.WriteInfo($"Started new chat session: {context.Title}");
            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/new" };
    }
}
