using GroqSharp.Commands.Handlers;
using GroqSharp.Commands.Interfaces;
using GroqSharp.Core;
using GroqSharp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGroqSharpCore(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddSingleton(config);

            // Register the configuration section explicitly
            services.AddSingleton(config.GetSection("Groq"));

            // Add conversation history (last 10 messages by default)
            services.AddSingleton<ConversationService>(_ => new ConversationService(10));

            services.AddHttpClient<IGroqClient, GroqClient>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            // Simplified registration - let DI handle the HttpClient
            services.AddTransient<IGroqService, GroqService>();

            return services;
        }

        public static IServiceCollection AddGroqSharpCommands(
            this IServiceCollection services)
        {
            services.AddTransient<ICommandProcessor, ExitCommandHandler>();
            services.AddTransient<ICommandProcessor, ExportCommandHandler>();
            services.AddTransient<ICommandProcessor, HelpCommandHandler>();
            services.AddTransient<ICommandProcessor, ModelsCommandHandler>();
            services.AddTransient<ICommandProcessor, ProcessCommandHandler>();
            services.AddTransient<ICommandProcessor, SetModelCommandHandler>();
            services.AddTransient<ICommandProcessor, StreamCommandHandler>();
            services.AddTransient<ICommandProcessor, HistoryCommandHandler>();
            services.AddTransient<ICommandProcessor, ClearCommandHandler>();

            services.AddSingleton<CommandDispatcher>();

            return services;
        }

        public static IServiceCollection AddGroqSharp(
            this IServiceCollection services,
            IConfiguration config)
        {
            return services
                .AddGroqSharpCore(config)
                .AddGroqSharpCommands();
        }
    }
}
