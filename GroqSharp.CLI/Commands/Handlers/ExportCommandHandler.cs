using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
                string content;
                string outputPath;

                if (args.Length > 0 && args[0].StartsWith("/"))
                {
                    // Interpret first argument as a piped command
                    var innerCommand = args[0];
                    var innerArgs = args.Skip(1).TakeWhile(arg =>
                        !arg.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
                        !arg.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                        !arg.EndsWith(".html", StringComparison.OrdinalIgnoreCase) &&
                        !arg.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)).ToArray();

                    outputPath = args.LastOrDefault(arg =>
                        arg.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                        arg.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                        arg.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                        arg.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        ?? context.Prompt("Enter output path: ");

                    var router = context.ServiceProvider.GetRequiredService<ICommandRouter>();
                    var input = string.Join(' ', new[] { innerCommand }.Concat(innerArgs));
                    var success = await router.RouteCommand(input, context);

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

                    outputPath = context.Prompt("Enter output path (default extension .txt): ");
                }

                FileProcessor.ExportToFile(content, outputPath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully exported to {outputPath}");
                Console.ResetColor();

                // Reset context result if not piped
                if (!(args.Length > 0 && args[0].StartsWith("/")))
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
    }
}
