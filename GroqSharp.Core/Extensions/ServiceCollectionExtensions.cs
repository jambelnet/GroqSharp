using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Configuration.Services;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Services;
using GroqSharp.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGroqSharpCore(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddSingleton(config);

            services.AddSingleton<IGroqConfigurationService, GroqConfigurationService>();
            services.Configure<GroqConfiguration>(config.GetSection("Groq"));

            services.AddSingleton<IGlobalConversationService, GlobalConversationService>();
            services.AddScoped<ConversationService>();

            services.AddHttpClient<IGroqClient, GroqClient>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            services.AddTransient<IGroqService, GroqService>();

            services.AddHttpClient<ITextToSpeechService, TextToSpeechService>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            services.AddHttpClient<ISpeechToTextService, SpeechToTextService>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            services.AddHttpClient<IVisionService, VisionService>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            return services;
        }
    }
}
