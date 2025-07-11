using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Constants;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using System.Globalization;
using System.Net.Http.Headers;

namespace GroqSharp.Core.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly IGroqConfigurationService _configService;
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public SpeechToTextService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _configService = config;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<string> TranscribeAudioAsync(string filePath, string? model = null)
        {
            using var content = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(filePath);
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

            var modelToUse = ModelSelector.Resolve(_modelResolver, GroqFeature.Transcribe, model);
            content.Add(new StringContent(modelToUse), "model");

            content.Add(new StringContent("verbose_json"), "response_format");
            var defaults = _configService.GetDefaultsFor(GroqFeature.Transcribe);
            content.Add(new StringContent(defaults.Temperature.ToString("0.0", CultureInfo.InvariantCulture)), "temperature");

            var language = string.IsNullOrWhiteSpace(_configService.GetConfiguration().WhisperLanguage) ? "en" : _configService.GetConfiguration().WhisperLanguage;
            content.Add(new StringContent(language), "language");

            var request = new HttpRequestMessage(HttpMethod.Post, GroqApiRoutes.AudioTranscriptions)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq STT API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
