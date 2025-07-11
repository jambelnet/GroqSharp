using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Builders;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.CLI.Services
{
    public static class CliAppRunner
    {
        public static async Task RunAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var groqService = scopedProvider.GetRequiredService<IGroqService>();
            var dispatcher = scopedProvider.GetRequiredService<CommandDispatcher>();
            var context = scopedProvider.GetRequiredService<CliSessionContext>();

            context.CurrentModel = scopedProvider.GetRequiredService<IConfiguration>()["Groq:DefaultModel"];

            await context.InitializeAsync(Guid.NewGuid().ToString());

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Groq API Client");
            Console.WriteLine("Available commands: " + string.Join(", ", dispatcher.GetAllCommands()));
            Console.ResetColor();

            try
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("\nYou: ");
                    Console.ResetColor();
                    var userInput = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(userInput))
                        continue;

                    if (userInput.StartsWith("/"))
                    {
                        await dispatcher.Dispatch(userInput, context);
                        if (context.ShouldExit) break;
                    }
                    else
                    {
                        await ProcessAiInput(userInput, context);
                    }
                }
            }
            finally
            {
                await context.SaveConversation();
            }
        }

        private static async Task ProcessAiInput(string input, CliSessionContext context)
        {
            try
            {
                context.Conversation.AddMessage(MessageRole.User, input);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("AI: ");
                Console.ResetColor();

                var request = new ChatRequestBuilder()
                    .WithModel(context.CurrentModel)
                    .WithMessages(context.GetSanitizedMessages())
                    .WithTemperature(0.7)
                    .WithMaxTokens(4096)
                    .Build();

                string response = await context.GroqService.GetChatCompletionAsync(request);
                context.Conversation.AddMessage(MessageRole.Assistant, response);
                await context.SaveConversation();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(response);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
