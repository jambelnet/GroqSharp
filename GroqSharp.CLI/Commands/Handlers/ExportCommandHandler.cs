using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Utilities;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ExportCommandHandler : ICommandProcessor
    {
        private readonly ICommandExecutor _executor;

        public ExportCommandHandler(ICommandExecutor executor)
        {
            _executor = executor;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/export", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                string content;
                string outputPath;

                bool isCommand = args.Length > 0 && args[0].StartsWith("/");

                if (isCommand)
                {
                    var innerCommand = args[0];
                    var innerArgs = args.Skip(1)
                        .TakeWhile(arg => !IsExportFilename(arg))
                        .ToArray();

                    outputPath = args.LastOrDefault(IsExportFilename)
                        ?? context.Prompt("Enter output path: ");

                    var input = string.Join(' ', new[] { innerCommand }.Concat(innerArgs));
                    var success = await _executor.ExecuteCommand(input, context);

                    if (!success || context.PreviousCommandResult is not string resultText)
                        throw new InvalidOperationException("Command did not return a string result.");

                    content = resultText;
                }
                else
                {
                    content = args.Length > 0
                        ? string.Join(" ", args)
                        : context.PreviousCommandResult as string
                        ?? context.Prompt("Enter AI output to export: ");

                    outputPath = context.Prompt("Enter output path (default: output.txt): ");
                }

                FileProcessor.ExportToFile(content, outputPath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Exported to: {outputPath}");
                Console.ResetColor();

                if (!isCommand)
                    context.PreviousCommandResult = null;
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

        private static bool IsExportFilename(string arg)
        {
            return arg.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                || arg.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                || arg.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                || arg.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }
    }
}
