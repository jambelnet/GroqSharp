using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using System.Text;

namespace GroqSharp.CLI.Services
{
    public class CommandDispatcher
    {
        private readonly List<ICommandProcessor> _commandProcessors;

        public CommandDispatcher(IEnumerable<ICommandProcessor> processors)
        {
            _commandProcessors = processors.ToList();
        }

        public async Task<bool> Dispatch(string input, CliSessionContext context)
        {
            if (!input.StartsWith("/"))
                return false;

            // Improved parsing that handles quoted arguments
            var parts = ParseCommandInput(input);
            var command = parts[0];
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            foreach (var processor in _commandProcessors)
            {
                if (await processor.ProcessCommand(command, args, context))
                    return true;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Unknown command: {command}");
            Console.ResetColor();

            return false;
        }

        private static string[] ParseCommandInput(string input)
        {
            var parts = new List<string>();
            var currentPart = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ' ' && !inQuotes)
                {
                    if (currentPart.Length > 0)
                    {
                        parts.Add(currentPart.ToString());
                        currentPart.Clear();
                    }
                    continue;
                }

                currentPart.Append(c);
            }

            // Add the last part
            if (currentPart.Length > 0)
            {
                parts.Add(currentPart.ToString());
            }

            return parts.ToArray();
        }

        public IEnumerable<string> GetAllCommands()
        {
            return _commandProcessors.SelectMany(p => p.GetAvailableCommands());
        }
    }
}
