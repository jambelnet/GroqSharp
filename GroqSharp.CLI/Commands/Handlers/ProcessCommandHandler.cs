using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;
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
                var filePath = args.Length > 0
                    ? args[0]
                    : context.Prompt("Enter file path to process: ");

                ConsoleOutputHelper.WriteInfo(
                    $"\nSupported formats: {string.Join(", ", FileProcessor.SupportedInputFormats)}\n" +
                    $"Max file size: {FileProcessor.MaxFileSize / (1024 * 1024)}MB");

                var textContent = FileProcessor.ExtractTextFromFile(filePath);

                ConsoleOutputHelper.WriteInfo($"\nFile content ({Path.GetFileName(filePath)}):");
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

                ConsoleOutputHelper.DisplayResponse(response);

                if (context.PromptYesNo("\nExport this response"))
                {
                    var outputPath = context.Prompt("Enter output path (default extension .txt): ");
                    if (!string.IsNullOrWhiteSpace(outputPath))
                    {
                        FileProcessor.ExportToFile(response, outputPath);
                        ConsoleOutputHelper.WriteInfo($"Exported to {outputPath}");
                    }
                }

                context.PreviousCommandResult = null;
            }
            catch (Exception ex)
            {
                context.PreviousCommandResult = null;
                ConsoleOutputHelper.WriteError($"Error: {ex.Message}");
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/process" };
    }
}
