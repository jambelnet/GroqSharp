using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;
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

                ConsoleOutputHelper.WriteInfo(
                    $"\nCurrent Model: {context.CurrentModel ?? Core.Services.ConversationService.DefaultModel}");

                ConsoleOutputHelper.WriteInfo("\nAvailable Models:");
                for (int i = 0; i < models.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{i + 1}. {models[i]}");
                    Console.ResetColor();
                }

                var input = context.Prompt($"Select model (1-{models.Count}): ");
                if (int.TryParse(input, out var choice) && choice >= 1 && choice <= models.Count)
                {
                    var selectedModel = models[choice - 1];
                    context.CurrentModel = selectedModel;
                    _modelConfig.SetModel(selectedModel);

                    ConsoleOutputHelper.WriteInfo($"Model changed to '{selectedModel}'.");
                }
                else
                {
                    ConsoleOutputHelper.WriteError("Invalid selection. Model not changed.");
                }
            }
            catch (Exception ex)
            {
                ConsoleOutputHelper.WriteError($"Error setting model: {ex.Message}");
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/setmodel" };
    }
}
