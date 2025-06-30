// Vision Command
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;
using System.Text.Json;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class VisionCommandHandler : ICommandProcessor
    {
        private readonly IVisionService _visionService;

        public VisionCommandHandler(IVisionService visionService)
        {
            _visionService = visionService;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/vision", StringComparison.OrdinalIgnoreCase))
                return false;

            var imagePath = args.Length > 0 ? args[0] : context.Prompt("Enter image path or URL: ");
            var prompt = args.Length > 1 ? string.Join(" ", args.Skip(1)) : context.Prompt("Enter prompt for image: ");

            try
            {
                var result = await _visionService.AnalyzeImageAsync(imagePath, prompt);

                // Parse the response to extract just the model's message content
                using var doc = JsonDocument.Parse(result);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nVision Response:\n" + content);
                Console.ResetColor();

                context.PreviousCommandResult = content ?? "(no content)";
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/vision" };
    }
}
