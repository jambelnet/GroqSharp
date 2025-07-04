using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class HistoryCommandHandler : ICommandProcessor
    {
        public Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/history", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            var history = context.Conversation.GetFullHistory();

            if (history.Count == 0)
                return Task.FromResult(ConsoleOutputHelper.ShowError("No conversation history available."));

            Console.WriteLine("\nConversation History:\n");

            foreach (var msg in history)
            {
                ConsoleOutputHelper.WriteMessageEntry(msg);

                if (msg.ExecutedTools?.Any() == true)
                {
                    foreach (var tool in msg.ExecutedTools)
                    {
                        ConsoleOutputHelper.WriteToolInfo(tool);
                    }
                }
            }

            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/history" };
    }
}
