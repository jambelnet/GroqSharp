using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
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

                string query = string.Join(" ", args.Where(x => !x.StartsWith("--")));
                if (string.IsNullOrWhiteSpace(query))
                    query = context.Prompt("Enter agentic query: ");

                if (string.IsNullOrWhiteSpace(query))
                    throw new ArgumentException("Prompt cannot be null or empty.", nameof(query));

                // Parse SearchSettings from args
                var searchSettings = new SearchSettings();

                string? excludeDomainsArg = args.FirstOrDefault(a => a.StartsWith("--exclude="));
                string? includeDomainsArg = args.FirstOrDefault(a => a.StartsWith("--include="));
                string? countryArg = args.FirstOrDefault(a => a.StartsWith("--country="));

                if (excludeDomainsArg != null)
                {
                    searchSettings.ExcludeDomains = excludeDomainsArg.Substring("--exclude=".Length)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                if (includeDomainsArg != null)
                {
                    searchSettings.IncludeDomains = includeDomainsArg.Substring("--include=".Length)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                if (countryArg != null)
                {
                    searchSettings.Country = countryArg.Substring("--country=".Length).Trim();
                }

                // Add user message
                context.Conversation.AddMessage(new Message
                {
                    Role = "user",
                    Content = query
                });

                // Prepare messages for API call, removing executed tools to avoid API errors
                var messages = context.GetSanitizedMessages();

                var request = new ChatRequest
                {
                    Model = _modelResolver.GetModelFor("/agent"),
                    Messages = messages,
                    SearchSettings = searchSettings
                };

                var response = await _groqService.GetStructuredResponseAsync(request);
                var message = response?.Choices?.FirstOrDefault()?.Message;

                if (message == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No response from Groq.");
                    Console.ResetColor();
                    return true;
                }

                // Patch executed tools before saving to history
                message.PatchExecutedTools(defaultInput: query);

                // Add full message including executed tools to conversation history (do NOT sanitize here)
                context.Conversation.AddMessage(message);
                context.PreviousCommandResult = message.Content?.ToString();

                // Show user-friendly response (hide executed tools from default output)
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n--- Response ---");
                Console.ResetColor();
                Console.WriteLine(message.Content?.ToString());

                // If verbose or summary flag, print executed tools info separately
                if (message.ExecutedTools?.Count > 0 && (verbose || summaryOnly))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n--- Executed Tools ---");
                    Console.ResetColor();

                    var tools = message.ExecutedTools;
                    if (summaryOnly)
                    {
                        var top = tools.First();
                        Console.WriteLine($"Tool: {top.ToolName}");
                        Console.WriteLine($"Output: {top.Output}");
                    }
                    else
                    {
                        foreach (var tool in tools)
                        {
                            Console.WriteLine($"Tool: {tool.ToolName}");
                            Console.WriteLine($"Input: {tool.Input}");
                            Console.WriteLine($"Output: {tool.Output}\n");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Agentic call failed: " + ex.Message);
                Console.ResetColor();
                return true;
            }
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/agent" };
    }
}
