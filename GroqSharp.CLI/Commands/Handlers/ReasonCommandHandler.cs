using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ReasonCommandHandler : ICommandProcessor
    {
        private readonly IReasoningService _reasoningService;
        private readonly IModelResolver _modelResolver;

        public ReasonCommandHandler(IReasoningService reasoningService, IModelResolver modelResolver)
        {
            _reasoningService = reasoningService;
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/reason", StringComparison.OrdinalIgnoreCase))
                return false;

            var prompt = args.Length > 0
                ? string.Join(" ", args)
                : context.Prompt("Enter reasoning prompt: ");

            context.Conversation.AddMessage(new Message { Role = MessageRole.User, Content = prompt });

            try
            {
                var model = ModelSelector.Resolve(_modelResolver, GroqFeature.Reasoning);
                var response = await _reasoningService.AnalyzeAsync(prompt, model);

                var extractedContent = OutputFormatter.ExtractChatCompletionContent(response);

                context.Conversation.AddMessage(new Message { Role = MessageRole.Assistant, Content = extractedContent });
                context.PreviousCommandResult = extractedContent;

                ConsoleOutputHelper.DisplayResponse(extractedContent);
            }
            catch (Exception ex)
            {
                ConsoleOutputHelper.WriteError("Reasoning failed: " + ex.Message);
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/reason" };
    }
}
