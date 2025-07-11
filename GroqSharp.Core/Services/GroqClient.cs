using GroqSharp.Core.Builders;
using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Constants;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GroqSharp.Core.Services
{
    public class GroqClient : IGroqClient
    {
        private readonly IModelResolver _modelResolver;
        private readonly HttpClient _httpClient;
        private readonly GroqConfiguration _settings;
        private readonly string _apiKey;

        private const string ChatCompletionsEndpoint = GroqApiRoutes.ChatCompletions;
        private const string ModelsEndpoint = "models";

        public GroqClient(HttpClient httpClient, IGroqConfigurationService configService, IModelResolver modelResolver)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _modelResolver = modelResolver ?? throw new ArgumentNullException(nameof(modelResolver));
            _settings = configService.GetConfiguration();
            _apiKey = _settings.ApiKey ?? throw new ArgumentException("API Key is required");
        }

        #region Chat Completion Methods

        public async Task<string> CompleteChatAsync(ChatRequest request)
        {
            request.Stream = false;
            return await ProcessChatRequestAsync(request);
        }

        public async Task<string> CompleteChatAsync(string userMessage)
        {
            var request = CreateBaseChatRequest(userMessage);
            request.Stream = false;
            return await ProcessChatRequestAsync(request);
        }

        public async Task<ChatCompletionResponse> CompleteStructuredChatAsync(ChatRequest request)
        {
            using var response = await SendChatRequestAsync(request, "application/json");
            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
            return result ?? new ChatCompletionResponse();
        }

        public async IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request)
        {
            request = request with { Stream = true }; // Ensure streaming
            await foreach (var chunk in ProcessStreamingRequestAsync(request))
            {
                yield return chunk;
            }
        }

        public async Task<string> CompleteChatStreamingAsync(ChatRequest request)
        {
            var builder = new StringBuilder();
            await foreach (var chunk in StreamChatCompletionAsync(request))
            {
                builder.Append(chunk);
            }
            return builder.ToString();
        }

        #endregion

        #region Model Methods

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            var fallbackModels = new List<string>
            {
                _modelResolver.GetModelFor(GroqFeature.Default),
                "llama3-70b-8192",
                "llama3-8b-8192"
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, ModelsEndpoint);
                if (!string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "temp")
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var modelResponse = await response.Content.ReadFromJsonAsync<ModelListResponse>();
                return modelResponse?.Data?.Select(m => m.Id).ToList() ?? fallbackModels;
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                Console.WriteLine($"Error fetching models: {ex.Message}");
                return fallbackModels;
            }
        }

        public Task<string> GetDefaultModelAsync()
        {
            return Task.FromResult(_modelResolver.GetModelFor(GroqFeature.Default));
        }

        #endregion

        #region Private Implementation

        private ChatRequest CreateBaseChatRequest(string userMessage)
        {
            var model = _modelResolver.GetModelFor(GroqFeature.Default);

            return new ChatRequestBuilder()
                .WithModel(model)
                .WithMessages(new[] { new Message { Role = MessageRole.User, Content = userMessage } })
                .WithTemperature(_settings.DefaultTemperature)
                .WithMaxTokens(_settings.DefaultMaxTokens)
                .Build();
        }

        private async Task<string> ProcessChatRequestAsync(ChatRequest request)
        {
            using var response = await SendChatRequestAsync(request, "application/json");

            if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
            {
                // Handle unexpected streaming response
                return await ProcessUnexpectedStreamResponseAsync(request);
            }

            var responseContent = await response.Content.ReadFromJsonAsync<ChatResponse>();
            return responseContent?.Choices?[0]?.Message?.Content ?? "No response content";
        }

        private async IAsyncEnumerable<string> ProcessStreamingRequestAsync(ChatRequest request)
        {
            using var response = await SendChatRequestAsync(request, "text/event-stream");
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

                var eventData = line["data: ".Length..];
                if (eventData == "[DONE]") yield break;

                var chunk = DeserializeResponseChunk(eventData);
                var content = chunk?.Choices?[0]?.Delta?.Content ?? chunk?.Choices?[0]?.Message?.Content;

                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }
            }
        }

        private async Task<HttpResponseMessage> SendChatRequestAsync(ChatRequest request, string acceptHeader)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsEndpoint)
            {
                Content = JsonContent.Create(request, options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));

            var response = await _httpClient.SendAsync(
                requestMessage,
                acceptHeader == "text/event-stream"
                    ? HttpCompletionOption.ResponseHeadersRead
                    : HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}");
            }

            return response;
        }

        private async Task<string> ProcessUnexpectedStreamResponseAsync(ChatRequest request)
        {
            var builder = new StringBuilder();
            await foreach (var chunk in ProcessStreamingRequestAsync(request with { Stream = true }))
            {
                builder.Append(chunk);
            }
            return builder.ToString();
        }

        private static ChatResponse? DeserializeResponseChunk(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<ChatResponse>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        #endregion
    }
}
