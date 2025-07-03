using GroqSharp.Core.Interfaces;
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Helpers;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class TranslateCommandHandler : ICommandProcessor
    {
        private readonly ITranslationService _translationService;
        private readonly IModelResolver _modelResolver;

        public TranslateCommandHandler(ITranslationService translationService, IModelResolver modelResolver)
        {
            _translationService = translationService;
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/translate", StringComparison.OrdinalIgnoreCase))
                return false;

            var filePath = args.FirstOrDefault() ?? context.Prompt("Enter audio file path to translate: ");

            try
            {
                var model = _modelResolver.GetModelFor(command);
                var content = await _translationService.TranslateAudioAsync(filePath, model);

                var extractedContent = OutputFormatter.ExtractChatCompletionContent(content);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTranslation Result:\n" + extractedContent);
                Console.ResetColor();

                context.PreviousCommandResult = extractedContent;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Translation failed: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/translate" };
    }
}
