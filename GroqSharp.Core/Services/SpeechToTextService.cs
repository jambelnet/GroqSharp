using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Interfaces;
using System.Net.Http.Headers;

namespace GroqSharp.Core.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public SpeechToTextService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<string> TranscribeAudioAsync(string filePath, string? model = null)
        {
            using var content = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(filePath);
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

            var modelToUse = model ?? _modelResolver.GetModelFor("/transcribe");
            content.Add(new StringContent(modelToUse), "model");

            content.Add(new StringContent("verbose_json"), "response_format");
            content.Add(new StringContent("0"), "temperature");

            var language = string.IsNullOrWhiteSpace(_settings.WhisperLanguage) ? "en" : _settings.WhisperLanguage;
            content.Add(new StringContent(language), "language");

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions")
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
