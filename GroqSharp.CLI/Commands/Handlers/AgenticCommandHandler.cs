using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class AgenticCommandHandler : ICommandProcessor
    {
        private readonly IGroqService _groqService;
        private readonly IModelResolver _modelResolver;

        public AgenticCommandHandler(IGroqService groqService, IModelResolver modelResolver)
        {
            _groqService = groqService;
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/agent", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                bool summaryOnly = args.Contains("--summary");
                bool verbose = args.Contains("--verbose");

                // Extract query
                string query = string.Join(" ", args.Where(x => !x.StartsWith("--")));
                if (string.IsNullOrWhiteSpace(query))
                    query = context.Prompt("Enter agentic query: ");

                if (string.IsNullOrWhiteSpace(query))
                    throw new ArgumentException("Prompt cannot be null or empty.", nameof(query));

                // Parse search filters
                var searchSettings = new SearchSettings
                {
                    ExcludeDomains = ExtractArgList(args, "--exclude="),
                    IncludeDomains = ExtractArgList(args, "--include="),
                    Country = ExtractArg(args, "--country=")
                };

                context.Conversation.AddMessage("user", query);

                var request = new ChatRequest
                {
                    Model = _modelResolver.GetModelFor("/agent"),
                    Messages = context.GetSanitizedMessages(),
                    SearchSettings = searchSettings
                };

                var response = await _groqService.GetStructuredResponseAsync(request);
                var message = response?.Choices?.FirstOrDefault()?.Message;

                if (message == null)
                    return ConsoleOutputHelper.ShowError("No response from Groq.");

                message.PatchExecutedTools(query);
                context.Conversation.AddMessage(message);
                context.PreviousCommandResult = message.Content;

                ConsoleOutputHelper.DisplayResponse(message.Content);
                ConsoleOutputHelper.DisplayExecutedTools(message.ExecutedTools, summaryOnly, verbose);

                return true;
            }
            catch (Exception ex)
            {
                return ConsoleOutputHelper.ShowError("Agentic call failed: " + ex.Message);
            }
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/agent" };

        private static string? ExtractArg(string[] args, string prefix) =>
            args.FirstOrDefault(a => a.StartsWith(prefix))?.Substring(prefix.Length).Trim();

        private static string[]? ExtractArgList(string[] args, string prefix) =>
            ExtractArg(args, prefix)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
