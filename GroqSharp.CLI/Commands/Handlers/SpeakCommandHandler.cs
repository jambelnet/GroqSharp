using GroqSharp.Core.Interfaces;
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;

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
                var model = ModelSelector.Resolve(_modelResolver, GroqFeature.Speak);
                var audio = await _ttsService.SynthesizeSpeechAsync(text);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = Path.Combine(Path.GetTempPath(), $"groq_tts_{timestamp}.wav");
                await File.WriteAllBytesAsync(fileName, audio);

                ConsoleOutputHelper.WriteInfo($"Saved audio to {fileName}");
            }
            catch (Exception ex)
            {
                ConsoleOutputHelper.WriteError("Text-to-speech failed: " + ex.Message);
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/speak" };
    }
}
