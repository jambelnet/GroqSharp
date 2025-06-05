using GroqSharp;
using GroqSharp.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GroqClient : IGroqClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _defaultModel;

    private const string ChatCompletionsEndpoint = "chat/completions";
    private const string ModelsEndpoint = "models";

    public GroqClient(HttpClient httpClient, IConfigurationSection groqConfig)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        (_apiKey, _defaultModel) = ParseConfiguration(groqConfig, "llama-3.3-70b-versatile");
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var fallbackModels = new List<string>
        {
            "llama-3.3-70b-versatile",
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

            return MarkDefaultModel(models, _defaultModel);
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

        return MarkDefaultModel(fallbackModels, _defaultModel);
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
            Messages = new[] { new Message { Role = "user", Content = userMessage } }
        };
        return await CompleteChatAsync(request);
    }

    private static List<string> MarkDefaultModel(List<string> models, string defaultModel)
    {
        return models.Select(m =>
            m == defaultModel ? $"{m} (Default)" : m
        ).ToList();
    }

    private static (string apiKey, string model) ParseConfiguration(IConfigurationSection config, string fallbackModel)
    {
        if (config == null)
        {
            return ("temp", fallbackModel);
        }

        var apiKey = config["ApiKey"] ?? throw new ArgumentException("API Key is missing in configuration");
        var model = config["DefaultModel"] ?? fallbackModel;
        return (apiKey, model);
    }
}
