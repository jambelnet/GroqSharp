using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using GroqSharp.CLI.Utilities;
using GroqSharp.Core.Enums;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class VisionCommandHandler : ICommandProcessor
    {
        private readonly IVisionService _visionService;
        private readonly IModelResolver _modelResolver;

        public VisionCommandHandler(IVisionService visionService, IModelResolver modelResolver)
        {
            _visionService = visionService;
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/vision", StringComparison.OrdinalIgnoreCase))
                return false;

            var imagePath = args.FirstOrDefault()
                ?? context.Prompt("Enter image path or URL: ");

            var prompt = args.Skip(1).Any()
                ? string.Join(" ", args.Skip(1))
                : context.Prompt("Enter prompt for image: ");

            context.Conversation.AddMessage(new Message
            {
                Role = MessageRole.User,
                Content = $"{imagePath} {prompt}".Trim()
            });

            try
            {
                var model = ModelSelector.Resolve(_modelResolver, GroqFeature.Vision);
                var response = await _visionService.AnalyzeImageAsync(imagePath, prompt, model);
                var extracted = OutputFormatter.ExtractChatCompletionContent(response);

                context.Conversation.AddMessage(new Message { Role = MessageRole.Assistant, Content = extracted });
                context.PreviousCommandResult = extracted;

                ConsoleOutputHelper.WriteInfo("\nVision Response:\n" + extracted);
            }
            catch (Exception ex)
            {
                ConsoleOutputHelper.WriteError("Vision analysis failed: " + ex.Message);
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/vision" };
    }
}
