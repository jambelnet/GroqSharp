using GroqSharp.CLI.Services;
using GroqSharp.Commands.Handlers;
using GroqSharp.Commands.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.Extensions
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
