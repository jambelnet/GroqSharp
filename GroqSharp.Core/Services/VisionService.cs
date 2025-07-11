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
    public class VisionService : IVisionService
    {
        private readonly IGroqConfigurationService _configService;
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public VisionService(HttpClient httpClient, IGroqConfigurationService config, IModelResolver modelResolver)
        {
            _httpClient = httpClient;
            _configService = config;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
            _modelResolver = modelResolver;
        }

        public async Task<string> AnalyzeImageAsync(string imagePathOrUrl, string prompt, string? model = null)
        {
            var modelToUse = ModelSelector.Resolve(_modelResolver, GroqFeature.Vision, model);
            var defaults = _configService.GetDefaultsFor(GroqFeature.Vision);
            var imageBlock = await BuildImageBlockAsync(imagePathOrUrl);

            var requestPayload = new
            {
                model = modelToUse,
                messages = new[]
                {
                    new
                    {
                        role = MessageRole.User,
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            imageBlock
                        }
                    }
                },
                temperature = defaults.Temperature,
                max_tokens = defaults.MaxTokens,
                top_p = defaults.TopP,
                stream = defaults.Stream
            };

            var json = JsonSerializer.Serialize(requestPayload, JsonDefaults.InWhenWritingNulldented);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, GroqApiRoutes.ChatCompletions)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq Vision API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static string GetMimeTypeFromExtension(string ext) => ext.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        private async Task<object> BuildImageBlockAsync(string pathOrUrl)
        {
            if (Uri.IsWellFormedUriString(pathOrUrl, UriKind.Absolute))
            {
                return new
                {
                    type = "image_url",
                    image_url = new { url = pathOrUrl }
                };
            }

            var bytes = await File.ReadAllBytesAsync(pathOrUrl);
            var base64 = Convert.ToBase64String(bytes);
            string mimeType = GetMimeTypeFromExtension(Path.GetExtension(pathOrUrl));

            return new
            {
                type = "image_url",
                image_url = new { url = $"data:{mimeType};base64,{base64}" }
            };
        }
    }
}
