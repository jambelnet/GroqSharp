// Speech-to-Text Command
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class TranscribeCommandHandler : ICommandProcessor
    {
        private readonly ISpeechToTextService _speechService;

        public TranscribeCommandHandler(ISpeechToTextService speechService)
        {
            _speechService = speechService;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/transcribe", StringComparison.OrdinalIgnoreCase))
                return false;

            var filePath = args.Length > 0 ? args[0] : context.Prompt("Enter audio file path: ");

            try
            {
                var result = await _speechService.TranscribeAudioAsync(filePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTranscription Result:\n" + result);
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

        public IEnumerable<string> GetAvailableCommands() => new[] { "/transcribe" };
    }
}
