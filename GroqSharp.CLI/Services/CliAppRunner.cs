using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using GroqSharp.Core.Services;
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
                context.Conversation.AddMessage("user", input);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("AI: ");
                Console.ResetColor();

                var request = new ChatRequest
                {
                    Model = context.CurrentModel ?? ConversationService.DefaultModel,
                    Messages = context.GetSanitizedMessages(),
                    Temperature = 0.7
                };

                string response = await context.GroqService.GetChatCompletionAsync(request);
                context.Conversation.AddMessage("assistant", response);
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
