using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ReasonCommandHandler : ICommandProcessor
    {
        private readonly IGroqService _groqService;

        public ReasonCommandHandler(IGroqService groqService)
        {
            _groqService = groqService;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/reason", StringComparison.OrdinalIgnoreCase))
                return false;

            var prompt = args.Length > 0 ? string.Join(" ", args) : context.Prompt("Enter reasoning prompt: ");

            context.Conversation.AddMessage(new Message
            {
                Role = "user",
                Content = prompt
            });

            try
            {
                var request = new ChatRequest
                {
                    Model = context.CurrentModel,
                    Messages = context.Conversation.GetApiMessages(),
                    Temperature = 0.6,
                    MaxTokens = 1024,
                    TopP = 0.95,
                    Stream = false
                };

                if (SupportsReasoningFormat(context.CurrentModel))
                    request.ReasoningFormat = "raw";

                if (SupportsReasoningEffort(context.CurrentModel))
                    request.ReasoningEffort = "default";

                var result = await _groqService.GetStructuredResponseAsync(request);
                var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "(no response)";

                context.Conversation.AddMessage(new Message
                {
                    Role = "assistant",
                    Content = content
                });

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nReasoning Output:\n" + content);
                Console.ResetColor();

                context.PreviousCommandResult = content;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Reasoning failed: " + ex.Message);
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/reason" };

        private static bool SupportsReasoningFormat(string model) =>
            model.Contains("deepseek", StringComparison.OrdinalIgnoreCase) ||
            model.Contains("qwen", StringComparison.OrdinalIgnoreCase);

        private static bool SupportsReasoningEffort(string model) =>
            model.Contains("qwen3-32b", StringComparison.OrdinalIgnoreCase);
    }
}
