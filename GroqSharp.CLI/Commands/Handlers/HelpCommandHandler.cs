using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class HelpCommandHandler : ICommandProcessor
    {
        public Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/help", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nAvailable commands:\n");

            PrintSection("Session Management", new[]
            {
                ("/new",        "Start a new chat session"),
                ("/history",    "Show previous conversation messages"),
                ("/clear",      "Clear the current session"),
                ("/archive",    "Manage saved conversations (list, load, delete, rename)")
            });

            PrintSection("Chat Interaction", new[]
            {
                ("/models",     "Show available models"),
                ("/setmodel",   "Change current model"),
                ("/stream",     "Start a streaming chat session"),
                ("/reason",     "Solve problems step-by-step using structured reasoning with memory"),
                ("/agent",      "Use agentic tools like search/code with compound-beta model")
            });

            PrintSection("Media & File Tools", new[]
            {
                ("/process",    "Process a local file"),
                ("/export",     "Export AI output to file"),
                ("/transcribe", "Transcribe audio file to text"),
                ("/translate",  "Translate non-English audio to English"),
                ("/speak",      "Convert text to speech and save audio"),
                ("/vision",     "Analyze image content using LLM")
            });

            PrintSection("System", new[]
            {
                ("/help",       "Show this help"),
                ("/exit",       "Quit the application")
            });

            Console.ResetColor();
            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/help" };

        private void PrintSection(string title, (string Command, string Description)[] commands)
        {
            Console.WriteLine(title + ":");
            foreach (var (cmd, desc) in commands)
            {
                Console.WriteLine($"  {cmd.PadRight(12)} - {desc}");
            }
            Console.WriteLine();
        }
    }
}
