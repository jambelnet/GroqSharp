using GroqSharp.Core.Interfaces;
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class SpeakCommandHandler : ICommandProcessor
    {
        private readonly ITextToSpeechService _ttsService;
        private readonly IModelResolver _modelResolver;

        public SpeakCommandHandler(ITextToSpeechService ttsService, IModelResolver modelResolver)
        {
            _ttsService = ttsService;
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/speak", StringComparison.OrdinalIgnoreCase))
                return false;

            var text = string.Join(" ", args);
            if (string.IsNullOrWhiteSpace(text))
                text = context.Prompt("Enter text to convert to speech: ");

            try
            {
                var model = _modelResolver.GetModelFor(command);
                var audio = await _ttsService.SynthesizeSpeechAsync(text);
                var fileName = Path.Combine(Path.GetTempPath(), "groq_tts.wav");
                await File.WriteAllBytesAsync(fileName, audio);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Saved audio to {fileName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/speak" };
    }
}
