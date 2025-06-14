using GroqSharp.Commands.Interfaces;
using GroqSharp.Commands.Models;
using GroqSharp.Models;
using GroqSharp.Services;
using System.Text;

namespace GroqSharp.Commands.Handlers
{
    public class StreamCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/stream", StringComparison.OrdinalIgnoreCase))
                return false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Starting streaming session. Type your messages, '/end' to stop.");
            Console.ResetColor();

            while (true)
            {
                var streamInput = context.Prompt("\nYou: ", ConsoleColor.Cyan);

                if (string.IsNullOrWhiteSpace(streamInput)) continue;
                if (streamInput.Equals("/end", StringComparison.OrdinalIgnoreCase)) break;

                try
                {
                    var request = new ChatRequest
                    {
                        Model = context.CurrentModel ?? ConversationService.DefaultModel,
                        Messages = new[] { new Message { Role = "user", Content = streamInput } },
                        Stream = true
                    };

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("AI: ");

                    var buffer = new StringBuilder();
                    await foreach (var chunk in context.GroqService.StreamChatCompletionAsync(request))
                    {
                        buffer.Append(chunk);
                        if (buffer.Length > 10 || chunk.EndsWith(' ') || chunk.EndsWith('\n'))
                        {
                            Console.Write(buffer.ToString());
                            buffer.Clear();
                        }
                    }
                    if (buffer.Length > 0)
                    {
                        Console.Write(buffer.ToString());
                    }
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
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
