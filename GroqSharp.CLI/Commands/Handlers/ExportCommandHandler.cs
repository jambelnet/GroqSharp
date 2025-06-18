using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ExportCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/export", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var content = args.Length > 0 ? string.Join(" ", args)
                            : context.PreviousCommandResult as string
                            ?? context.Prompt("Enter AI output to export: ");

                var outputPath = context.Prompt("Enter output path (default extension .txt): ");
                if (string.IsNullOrWhiteSpace(outputPath))
                    return true;
                var format = Path.GetExtension(outputPath).TrimStart('.');
                FileProcessor.ExportToFile(content, outputPath);

                // Only clear if we used the previous result
                if (args.Length == 0 && context.PreviousCommandResult != null)
                {
                    context.PreviousCommandResult = null;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully exported to {outputPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Export failed: {ex.Message}");
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/export" };
    }
}
