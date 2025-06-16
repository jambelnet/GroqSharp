using GroqSharp.Core;
using GroqSharp.Interfaces;
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
            services.AddSingleton<IGroqConfigurationService, GroqConfigurationService>();
            services.AddSingleton<ConversationService>(_ => new ConversationService(10));

            services.AddHttpClient<IGroqClient, GroqClient>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            services.AddTransient<IGroqService, GroqService>();

            return services;
        }
    }
}
