using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Services;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class SetModelCommandHandler : ICommandProcessor
    {
        private readonly ModelConfigurationService _modelConfig;

        public SetModelCommandHandler(ModelConfigurationService modelConfig)
        {
            _modelConfig = modelConfig;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/setmodel", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var models = await context.GroqService.GetAvailableModelsAsync();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nCurrent Model: {context.CurrentModel ?? ConversationService.DefaultModel}");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nAvailable Models:");
                for (int i = 0; i < models.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{i + 1}. {models[i]}");
                }
                Console.ResetColor();

                var input = context.Prompt($"Select model (1-{models.Count}): ");
                if (int.TryParse(input, out var choice) && choice >= 1 && choice <= models.Count)
                {
                    var selectedModel = models[choice - 1];
                    context.CurrentModel = selectedModel;
                    _modelConfig.SetModel(selectedModel);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Model changed to '{selectedModel}'.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid selection. Model not changed.");
                }

                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error setting model: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/setmodel" };
    }
}
