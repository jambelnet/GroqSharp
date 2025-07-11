using GroqSharp.Core.Builders;
using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Constants;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GroqSharp.Core.Services
{
    public class ReasoningService : IReasoningService
    {
        private readonly IGroqConfigurationService _configService;
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public ReasoningService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _configService = config;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<string> AnalyzeAsync(string prompt, string? model = null, string reasoningFormat = "raw", string reasoningEffort = "default")
        {
            var usedModel = ModelSelector.Resolve(_modelResolver, GroqFeature.Reasoning, model);
            var defaults = _configService.GetDefaultsFor(GroqFeature.Reasoning);

            var request = new ChatRequestBuilder()
                .WithModel(usedModel)
                .WithMessages(new[] { new Message { Role = MessageRole.User, Content = prompt } })
                .WithTemperature(defaults.Temperature)
                .WithMaxTokens(defaults.MaxTokens)
                .WithTopP(defaults.TopP)
                .WithStream(false)
                .WithReasoningFormat(reasoningFormat)
                .WithReasoningEffort(usedModel.Contains("qwen") ? reasoningEffort : null)
                .Build();

            var json = JsonSerializer.Serialize(request, JsonDefaults.InWhenWritingNulldented);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, GroqApiRoutes.ChatCompletions)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq Reasoning API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
