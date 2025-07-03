using GroqSharp.Core.Interfaces;
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Helpers;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class TranscribeCommandHandler : ICommandProcessor
    {
        private readonly ISpeechToTextService _speechToTextService;
        private readonly IModelResolver _modelResolver;

        public TranscribeCommandHandler(ISpeechToTextService speechToTextService, IModelResolver modelResolver)
        {
            _speechToTextService = speechToTextService;
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/transcribe", StringComparison.OrdinalIgnoreCase))
                return false;

            var filePath = args.Length > 0 ? args[0] : context.Prompt("Enter audio file path: ");

            try
            {
                var model = _modelResolver.GetModelFor(command);
                var content = await _speechToTextService.TranscribeAudioAsync(filePath, model);

                var extractedContent = OutputFormatter.ExtractChatCompletionContent(content);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTranscription Result:\n" + extractedContent);
                Console.ResetColor();

                context.PreviousCommandResult = extractedContent;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/transcribe" };
    }
}
