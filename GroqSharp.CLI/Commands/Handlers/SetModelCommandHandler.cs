using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class SetModelCommandHandler : ICommandProcessor
    {
        private readonly string _appSettingsPath;

        public SetModelCommandHandler(string appSettingsPath = "appsettings.json")
        {
            _appSettingsPath = appSettingsPath;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/setmodel", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var models = await context.GroqService.GetAvailableModelsAsync();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nAvailable Models:");
                for (int i = 0; i < models.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{i + 1}. {models[i]}");
                }
                Console.ResetColor();

                var input = context.Prompt($"Select model (1-{models.Count}): ");

                if (int.TryParse(input, out var choice) && choice > 0 && choice <= models.Count)
                {
                    context.CurrentModel = models[choice - 1];
                    await UpdateModelInAppSettingsAsync(context.CurrentModel);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Model changed to '{context.CurrentModel}'.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid selection. Model not changed.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error setting model: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        private async Task UpdateModelInAppSettingsAsync(string newModel)
        {
            if (string.IsNullOrWhiteSpace(newModel))
                throw new ArgumentException("Model name cannot be empty");

            string tempPath = Path.GetTempFileName();
            string originalPath = _appSettingsPath;

            try
            {
                // Read existing content
                string jsonText;
                using (var fileStream = new FileStream(originalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fileStream))
                {
                    jsonText = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    Console.WriteLine("appsettings.json is empty");
                    return;
                }

                // Parse JSON
                var jsonNode = JsonNode.Parse(jsonText);
                if (jsonNode == null)
                {
                    Console.WriteLine("Failed to parse appsettings.json");
                    return;
                }

                // Get or create Groq section
                var groqSection = jsonNode["Groq"]?.AsObject() ?? new JsonObject();
                jsonNode["Groq"] = groqSection; // Ensure it exists

                // Update the model value
                groqSection["DefaultModel"] = JsonValue.Create(newModel);

                // Write to temp file first
                await using (var tempStream = new FileStream(tempPath, FileMode.Create))
                await using (var writer = new StreamWriter(tempStream))
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    await writer.WriteAsync(jsonNode.ToJsonString(options));
                }

                // Atomic replace operation
                File.Replace(tempPath, originalPath, null);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Model updated to '{newModel}' in appsettings.json");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error updating model: {ex.Message}");
                Console.ResetColor();

                // Clean up temp file if something went wrong
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                throw;
            }
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/setmodel" };
    }
}
