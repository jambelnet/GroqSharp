using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ProcessCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/process", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var filePath = args.Length > 0 ? args[0] : context.Prompt("Enter file path to process: ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nSupported formats: {string.Join(", ", FileProcessor.SupportedInputFormats)}");
                Console.WriteLine($"Max file size: {FileProcessor.MaxFileSize / (1024 * 1024)}MB");
                Console.ResetColor();

                var textContent = FileProcessor.ExtractTextFromFile(filePath);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nFile content ({Path.GetFileName(filePath)}):");
                Console.ResetColor();

                Console.WriteLine(textContent.Length > 500
                    ? textContent.Substring(0, 500) + "..."
                    : textContent);

                if (!context.PromptYesNo("\nProcess this content"))
                {
                    context.PreviousCommandResult = null;
                    return true;
                }

                var response = await context.GroqService.GetChatCompletionAsync(
                    $"Process this document content:\n{textContent}");

                context.PreviousCommandResult = response;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nAI Response:\n{response}");
                Console.ResetColor();

                if (context.PromptYesNo("\nExport this response"))
                {
                    var outputPath = context.Prompt("Enter output path (default extension .txt): ");
                    if (!string.IsNullOrWhiteSpace(outputPath))
                    {
                        FileProcessor.ExportToFile(response, outputPath);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Exported to {outputPath}");
                        Console.ResetColor();
                    }
                }

                context.PreviousCommandResult = null;
            }
            catch (Exception ex)
            {
                context.PreviousCommandResult = null;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/process" };
    }
}
