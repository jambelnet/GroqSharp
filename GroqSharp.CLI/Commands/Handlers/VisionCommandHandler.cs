using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

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

            var imagePath = args.Length > 0 ? args[0] : context.Prompt("Enter image path or URL: ");
            var prompt = args.Length > 1 ? string.Join(" ", args.Skip(1)) : context.Prompt("Enter prompt for image: ");

            context.Conversation.AddMessage(new Message
            {
                Role = "user",
                Content = $"{imagePath} {prompt}".Trim()
            });

            try
            {
                var model = _modelResolver.GetModelFor(command);
                var content = await _visionService.AnalyzeImageAsync(imagePath, prompt, model);

                var extractedContent = OutputFormatter.ExtractChatCompletionContent(content);

                context.Conversation.AddMessage(new Message { Role = "assistant", Content = extractedContent });

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nVision Response:\n" + extractedContent);
                Console.ResetColor();

                context.PreviousCommandResult = extractedContent;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/vision" };
    }
}
