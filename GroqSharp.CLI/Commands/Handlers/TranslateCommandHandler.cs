using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;
using System.Text.Json;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class TranslateCommandHandler : ICommandProcessor
    {
        private readonly ITranslationService _translationService;

        public TranslateCommandHandler(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/translate", StringComparison.OrdinalIgnoreCase))
                return false;

            var path = args.FirstOrDefault() ?? context.Prompt("Enter audio file path to translate: ");

            try
            {
                var result = await _translationService.TranslateAudioAsync(path);

                string? text = TryExtractContent(result);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTranslation Result:\n" + (text ?? result));
                Console.ResetColor();

                context.PreviousCommandResult = text ?? result;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Translation failed: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        private static string? TryExtractContent(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textProp))
                    return textProp.GetString();
            }
            catch { }

            return null;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/translate" };
    }
}
