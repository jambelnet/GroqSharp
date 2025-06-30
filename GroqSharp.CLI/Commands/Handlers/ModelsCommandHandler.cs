using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

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
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Fetching available models...");
                Console.ResetColor();

                var models = await context.GroqService.GetAvailableModelsAsync();
                var defaultModel = await context.GroqService.GetDefaultModelAsync();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nAvailable Models:");
                foreach (var model in models)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"- {model}");
                }
                Console.WriteLine($"\nDefault: {defaultModel}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error fetching models: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/models" };
    }
}
