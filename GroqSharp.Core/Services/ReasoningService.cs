using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GroqSharp.Core.Services
{
    public class ReasoningService : IReasoningService
    {
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public ReasoningService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<string> AnalyzeAsync(string prompt, string? model = null, string reasoningFormat = "raw", string reasoningEffort = "default")
        {
            var usedModel = model ?? _modelResolver.GetModelFor("/reason");

            var requestPayload = new
            {
                model = usedModel,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.6,
                max_tokens = 1024,
                top_p = 0.95,
                stream = false,
                reasoning_format = reasoningFormat,
                reasoning_effort = usedModel.Contains("qwen") ? reasoningEffort : null
            };

            var json = JsonSerializer.Serialize(requestPayload, JsonDefaults.InWhenWritingNulldented);

            var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq Reasoning API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
