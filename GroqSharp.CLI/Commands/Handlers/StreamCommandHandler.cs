using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Utilities;
using GroqSharp.Core.Builders;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using System.Text;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class StreamCommandHandler : ICommandProcessor
    {
        private readonly IModelResolver _modelResolver;

        public StreamCommandHandler(IModelResolver modelResolver)
        {
            _modelResolver = modelResolver;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/stream", StringComparison.OrdinalIgnoreCase))
                return false;

            ConsoleOutputHelper.WriteInfo("Streaming session started. Type your message. Type '/end' to stop.");

            while (true)
            {
                var input = context.Prompt("\nYou: ", ConsoleColor.Cyan);
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("/end", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    context.Conversation.AddMessage(new Message { Role = MessageRole.User, Content = input });

                    var request = new ChatRequestBuilder()
                        .WithModel(context.CurrentModel ?? _modelResolver.GetModelFor(GroqFeature.Default))
                        .WithMessages(context.GetSanitizedMessages())
                        .WithStream(true)
                        .WithMaxTokens(4096)
                        .Build();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("AI: ");

                    var buffer = new StringBuilder();
                    var assistantResponseBuilder = new StringBuilder();

                    await foreach (var chunk in context.GroqService.StreamChatCompletionAsync(request))
                    {
                        buffer.Append(chunk);
                        assistantResponseBuilder.Append(chunk);

                        if (buffer.Length > 10 || chunk.EndsWith(' ') || chunk.EndsWith('\n'))
                        {
                            Console.Write(buffer.ToString());
                            buffer.Clear();
                        }
                    }

                    if (buffer.Length > 0)
                        Console.Write(buffer.ToString());

                    Console.WriteLine();

                    var assistantResponse = assistantResponseBuilder.ToString();
                    context.Conversation.AddMessage(new Message { Role = MessageRole.Assistant, Content = assistantResponse });
                }
                catch (Exception ex)
                {
                    ConsoleOutputHelper.WriteError("Streaming failed: " + ex.Message);
                }
                finally
                {
                    Console.ResetColor();
                }
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/stream" };
    }
}
