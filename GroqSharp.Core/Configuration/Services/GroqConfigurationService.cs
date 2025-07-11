using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Models;
using Microsoft.Extensions.Configuration;

namespace GroqSharp.Core.Configuration.Services
{
    public class GroqConfigurationService : IGroqConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly GroqConfiguration _settings;

        public GroqConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _settings = _configuration.GetSection("Groq").Get<GroqConfiguration>()
                        ?? throw new InvalidOperationException("Groq configuration is missing or malformed.");

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("Groq API key is missing in configuration.");
        }

        public GroqConfiguration GetConfiguration() => _settings;

        public RequestDefaults GetDefaultsFor(GroqFeature feature)
        {
            return feature switch
            {
                GroqFeature.Reasoning => new RequestDefaults
                {
                    Temperature = _settings.DefaultReasoningTemperature,
                    MaxTokens = _settings.DefaultReasoningMaxTokens,
                    TopP = _settings.DefaultReasoningTopP,
                    Stream = false
                },
                GroqFeature.Vision => new RequestDefaults
                {
                    Temperature = _settings.DefaultVisionTemperature,
                    MaxTokens = _settings.DefaultMaxTokens,
                    TopP = 1,
                    Stream = false
                },
                GroqFeature.Transcribe or GroqFeature.Translate => new RequestDefaults
                {
                    Temperature = 0,
                    MaxTokens = 1024,
                    TopP = 1,
                    Stream = false
                },
                _ => new RequestDefaults
                {
                    Temperature = _settings.DefaultTemperature,
                    MaxTokens = _settings.DefaultMaxTokens,
                    TopP = _settings.DefaultTopP,
                    Stream = false
                }
            };
        }
    }
}
