using GroqSharp;
using GroqSharp.Configuration;
using GroqSharp.Core;
using GroqSharp.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GroqClient : IGroqClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _defaultModel;
    private readonly GroqSettings _settings;

    private const string ChatCompletionsEndpoint = "chat/completions";
    private const string ModelsEndpoint = "models";

    public GroqClient(HttpClient httpClient, IOptions<GroqSettings> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _apiKey = _settings.ApiKey ?? throw new ArgumentException("API Key is required");
        _defaultModel = _settings.DefaultModel ?? GroqConstants.DefaultModel;
    }

    public async Task<string> CompleteChatAsync(ChatRequest request)
    {
        try
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
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<ChatResponse>();
            return responseContent?.Choices?[0]?.Message?.Content ?? "No response content";
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP request failed: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected API error: {ex.Message}");
            throw;
        }
    }

    public async Task<string> CompleteChatAsync(string userMessage)
    {
        var request = new ChatRequest
        {
            Model = _defaultModel,
            Messages = new[] { new Message { Role = "user", Content = userMessage } },
            Temperature = _settings.DefaultTemperature,
            MaxTokens = _settings.DefaultMaxTokens
        };
        return await CompleteChatAsync(request);
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var fallbackModels = new List<string>
        {
            GroqConstants.DefaultModel,
            "llama3-70b-8192",
            "llama3-8b-8192"
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ModelsEndpoint);

            if (_apiKey != "temp")
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var modelResponse = await response.Content.ReadFromJsonAsync<ModelListResponse>();
            var models = modelResponse?.Data?.Select(m => m.Id).ToList() ?? fallbackModels;

            return models;
            //return MarkDefaultModel(models, _defaultModel);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network/API error while fetching models: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error while fetching models: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error while fetching models: {ex.Message}");
        }

        return fallbackModels;
        //return MarkDefaultModel(fallbackModels, _defaultModel);
    }

    public async Task<string> GetDefaultModelAsync()
    {
        var availableModels = await GetAvailableModelsAsync();

        // Verify the default model exists in available models
        if (!availableModels.Contains(_defaultModel))
        {
            Console.WriteLine($"Warning: Configured default model '{_defaultModel}' not found in available models");
        }

        return _defaultModel;
    }

    public async IAsyncEnumerable<string> StreamChatCompletionAsync(ChatRequest request)
    {
        // Make a copy of the request to avoid modifying the original
        var streamRequest = new ChatRequest
        {
            Model = request.Model,
            Messages = request.Messages,
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            TopP = request.TopP,
            Stream = true, // Explicitly set streaming
            Stop = request.Stop,
            FrequencyPenalty = request.FrequencyPenalty,
            PresencePenalty = request.PresencePenalty
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(streamRequest, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            })
        };

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string line;
            try
            {
                line = await reader.ReadLineAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stream read error: {ex.Message}");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var eventData = line["data: ".Length..];
            if (eventData == "[DONE]")
                yield break;

            ChatResponse chunk = null;
            try
            {
                chunk = JsonSerializer.Deserialize<ChatResponse>(eventData);
            }
            catch (JsonException)
            {
                continue;
            }

            var content = chunk?.Choices?[0]?.Delta?.Content
                    ?? chunk?.Choices?[0]?.Message?.Content;

            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    private static List<string> MarkDefaultModel(List<string> models, string defaultModel)
    {
        return models.Select(m =>
            m == defaultModel ? $"{m} (Default)" : m
        ).ToList();
    }
}
