using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ModelsCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/models", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                ConsoleOutputHelper.WriteInfo("Fetching available models...");

                var models = await context.GroqService.GetAvailableModelsAsync();
                var defaultModel = await context.GroqService.GetDefaultModelAsync();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nAvailable Models:");

                foreach (var model in models)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"- {model}");
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nDefault Model: {defaultModel}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                ConsoleOutputHelper.WriteError($"Error fetching models: {ex.Message}");
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/models" };
    }
}
