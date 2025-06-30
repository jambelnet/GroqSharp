// Text-to-Speech Command
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class SpeakCommandHandler : ICommandProcessor
    {
        private readonly ITextToSpeechService _tts;

        public SpeakCommandHandler(ITextToSpeechService tts)
        {
            _tts = tts;
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
                var audio = await _tts.SynthesizeSpeechAsync(text);
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
