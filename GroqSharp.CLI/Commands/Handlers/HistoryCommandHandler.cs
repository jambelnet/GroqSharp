using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Models;

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
            {
                Console.WriteLine("No conversation history available.");
                return Task.FromResult(true);
            }

            Console.WriteLine("\nConversation History:\n");

            foreach (var msg in history)
            {
                Console.ForegroundColor = msg.Role == "user" ? ConsoleColor.Cyan : ConsoleColor.Green;
                Console.WriteLine($"[{msg.Role}] {msg.Content}");

                if (msg.ExecutedTools?.Any() == true)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    foreach (ExecutedTool tool in msg.ExecutedTools)
                    {
                        Console.WriteLine($"  Tool: {tool.ToolName}");
                        Console.WriteLine($"  Input: {tool.Input}");
                        Console.WriteLine($"  Output: {tool.Output}");
                        if (tool.Score.HasValue)
                            Console.WriteLine($"  Score: {tool.Score.Value:F4}");
                    }
                }

                Console.ResetColor();
            }

            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/history" };
    }
}
