using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Interfaces;
using System.Net.Http.Headers;

namespace GroqSharp.Core.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public TranslationService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<string> TranslateAudioAsync(string filePath, string? model = null)
        {
            using var content = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(filePath);
            content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

            // Use model param or fallback to resolver
            var modelToUse = model ?? _modelResolver.GetModelFor("/translate");
            content.Add(new StringContent(modelToUse), "model");

            content.Add(new StringContent("json"), "response_format");
            content.Add(new StringContent("0"), "temperature");

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/translations")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq Translate API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
