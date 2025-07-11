using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GroqSharp.Core.Services
{
    public class TextToSpeechService : ITextToSpeechService
    {
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public TextToSpeechService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<byte[]> SynthesizeSpeechAsync(string text, string voice = "Arista-PlayAI")
        {
            var payload = new
            {
                model = ModelSelector.Resolve(_modelResolver, GroqFeature.Speak),
                input = text,
                voice,
                response_format = "wav"
            };

            var json = JsonSerializer.Serialize(payload, JsonDefaults.InWhenWritingNulldented);

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/speech")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq TTS API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
