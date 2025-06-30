using GroqSharp.CLI.Commands.Handlers;
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Routing;
using GroqSharp.CLI.Commands.Services;
using GroqSharp.CLI.Services;
using GroqSharp.Core.Extensions;
using GroqSharp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.CLI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGroqSharpCommands(this IServiceCollection services)
        {
            // Core/shared services
            services.AddSingleton<ModelConfigurationService>();

            // Command handlers (alphabetical order for easy maintenance)
            services.AddTransient<ICommandProcessor, ArchiveCommandHandler>();
            services.AddTransient<ICommandProcessor, ClearCommandHandler>();
            services.AddTransient<ICommandProcessor, ExitCommandHandler>();
            services.AddTransient<ICommandProcessor, ExportCommandHandler>();
            services.AddTransient<ICommandProcessor, HelpCommandHandler>();
            services.AddTransient<ICommandProcessor, HistoryCommandHandler>();
            services.AddTransient<ICommandProcessor, ModelsCommandHandler>();
            services.AddTransient<ICommandProcessor, NewCommandHandler>();
            services.AddTransient<ICommandProcessor, ProcessCommandHandler>();
            services.AddTransient<ICommandProcessor, ReasonCommandHandler>();
            services.AddTransient<ICommandProcessor, SetModelCommandHandler>();
            services.AddTransient<ICommandProcessor, SpeakCommandHandler>();
            services.AddTransient<ICommandProcessor, StreamCommandHandler>();
            services.AddSingleton<ICommandProcessor, TranslateCommandHandler>();
            services.AddTransient<ICommandProcessor, TranscribeCommandHandler>();
            services.AddTransient<ICommandProcessor, VisionCommandHandler>();

            // Routing/dispatch
            services.AddSingleton<ICommandRouter, CommandRouter>();
            services.AddSingleton<ICommandExecutor, CommandExecutor>();
            services.AddSingleton<CommandDispatcher>();

            return services;
        }

        public static IServiceCollection AddGroqSharp(this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddGroqSharpCore(config)
                .AddGroqSharpCommands();
        }
    }
}
