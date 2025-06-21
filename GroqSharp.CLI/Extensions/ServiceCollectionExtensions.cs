using GroqSharp.CLI.Commands.Handlers;
using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Services;
using GroqSharp.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.CLI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGroqSharpCommands(this IServiceCollection services)
        {
            services.AddTransient<ICommandProcessor, ArchiveCommandHandler>();
            services.AddTransient<ICommandProcessor, ClearCommandHandler>();
            services.AddTransient<ICommandProcessor, ExitCommandHandler>();
            services.AddTransient<ICommandProcessor, ExportCommandHandler>();
            services.AddTransient<ICommandProcessor, HelpCommandHandler>();
            services.AddTransient<ICommandProcessor, HistoryCommandHandler>();
            services.AddTransient<ICommandProcessor, ModelsCommandHandler>();
            services.AddTransient<ICommandProcessor, NewCommandHandler>();
            services.AddTransient<ICommandProcessor, ProcessCommandHandler>();
            services.AddTransient<ICommandProcessor, SetModelCommandHandler>();
            services.AddTransient<ICommandProcessor, StreamCommandHandler>();
            services.AddTransient<ICommandProcessor, TranscribeCommandHandler>();
            services.AddTransient<ICommandProcessor, SpeakCommandHandler>();
            services.AddTransient<ICommandProcessor, VisionCommandHandler>();

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
