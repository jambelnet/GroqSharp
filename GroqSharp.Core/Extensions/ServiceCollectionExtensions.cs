using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Configuration.Services;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGroqSharpCore(this IServiceCollection services, IConfiguration config)
        {
            // Configuration
            services.AddSingleton(config);
            services.AddSingleton<IModelResolver, ModelResolver>();
            services.Configure<GroqConfiguration>(config.GetSection("Groq"));
            services.AddSingleton<IGroqConfigurationService, GroqConfigurationService>();
            services.AddSingleton<ModelConfigurationService>();

            // Core Services
            services.AddSingleton<IGlobalConversationService, GlobalConversationService>();
            services.AddScoped<ConversationService>();
            services.AddTransient<IGroqService, GroqService>();

            // HTTP Clients (all using same base URL)
            var baseUri = new Uri(config["Groq:BaseUrl"]);

            services.AddHttpClient<IGroqClient, GroqClient>(client => client.BaseAddress = baseUri);
            services.AddHttpClient<ITextToSpeechService, TextToSpeechService>(client => client.BaseAddress = baseUri);
            services.AddHttpClient<ISpeechToTextService, SpeechToTextService>(client => client.BaseAddress = baseUri);
            services.AddHttpClient<IVisionService, VisionService>(client => client.BaseAddress = baseUri);
            services.AddHttpClient<ITranslationService, TranslationService>(client => client.BaseAddress = baseUri);
            services.AddHttpClient<IReasoningService, ReasoningService>(client => client.BaseAddress = baseUri);

            return services;
        }
    }
}
