using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GroqSharp.Core.Services
{
    public class VisionService : IVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        public VisionService(HttpClient httpClient, IGroqConfigurationService config)
        {
            _httpClient = httpClient;
            _settings = config.GetConfiguration();
            _apiKey = _settings.ApiKey;
        }

        public async Task<string> AnalyzeImageAsync(string imagePathOrUrl, string prompt)
        {
            bool isUrl = Uri.IsWellFormedUriString(imagePathOrUrl, UriKind.Absolute);
            string model = _settings.DefaultVisionModel ?? "meta-llama/llama-4-scout-17b-16e-instruct";

            object imageBlock;
            if (isUrl)
            {
                imageBlock = new
                {
                    type = "image_url",
                    image_url = new { url = imagePathOrUrl }
                };
            }
            else
            {
                var bytes = await File.ReadAllBytesAsync(imagePathOrUrl);
                var base64 = Convert.ToBase64String(bytes);
                string mimeType = GetMimeTypeFromExtension(Path.GetExtension(imagePathOrUrl));

                imageBlock = new
                {
                    type = "image_url",
                    image_url = new { url = $"data:{mimeType};base64,{base64}" }
                };
            }

            var requestPayload = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            imageBlock
                        }
                    }
                },
                temperature = 1,
                max_tokens = 1024,
                top_p = 1,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
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

        private static string GetMimeTypeFromExtension(string ext)
        {
            return ext.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
