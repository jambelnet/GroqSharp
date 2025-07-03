using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Models;
using GroqSharp.Core.Services;
using System.Text;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class StreamCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/stream", StringComparison.OrdinalIgnoreCase))
                return false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Streaming session started. Type your message. Type '/end' to stop.");
            Console.ResetColor();

            while (true)
            {
                var input = context.Prompt("\nYou: ", ConsoleColor.Cyan);

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("/end", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    // Add user message to conversation history
                    context.Conversation.AddMessage(new Message { Role = "user", Content = input });

                    // Use the CurrentModel from context or fallback to default since ConversationService does not have GetModelFor method
                    var model = context.CurrentModel ?? ConversationService.DefaultModel;

                    var request = new ChatRequest
                    {
                        Model = model,
                        Messages = context.GetSanitizedMessages(),
                        Stream = true
                    };

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

                    // Add assistant response to conversation history
                    var assistantResponse = assistantResponseBuilder.ToString();
                    context.Conversation.AddMessage(new Message { Role = "assistant", Content = assistantResponse });
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Streaming failed: " + ex.Message);
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
