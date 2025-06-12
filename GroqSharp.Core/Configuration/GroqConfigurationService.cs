using Microsoft.Extensions.Configuration;
using GroqSharp.Models;
using GroqSharp.Interfaces;

namespace GroqSharp.Services
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
    }
}
