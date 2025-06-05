using GroqSharp;
using GroqSharp.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

public class GroqClient : IGroqClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _defaultModel;

    public GroqClient(HttpClient httpClient, IConfigurationSection groqConfig)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Special handling for setup scenario
        if (groqConfig == null)
        {
            _apiKey = "temp"; // Will be replaced in API calls during setup
            _defaultModel = "llama-3.3-70b-versatile";
            return;
        }

        _apiKey = groqConfig["ApiKey"] ?? throw new ArgumentException("API Key is missing in configuration");
        _defaultModel = groqConfig["DefaultModel"] ?? "llama-3.3-70b-versatile";
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
            var request = new HttpRequestMessage(HttpMethod.Get, "models");

            if (_apiKey != "temp") // Skip auth during setup
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var modelResponse = await response.Content.ReadFromJsonAsync<ModelListResponse>();

            var models = modelResponse?.Data?.Select(m => m.Id).ToList() ?? fallbackModels;

            return MarkDefaultModel(models, _defaultModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not fetch models from API. Error: {ex.Message}");
            return MarkDefaultModel(fallbackModels, _defaultModel);
        }
    }

    private List<string> MarkDefaultModel(List<string> models, string defaultModel)
    {
        return models.Select(m =>
            m == defaultModel ? $"{m} (Default)" : m
        ).ToList();
    }

    public async Task<string> CompleteChatAsync(ChatRequest request)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
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
        catch (Exception ex)
        {
            Console.WriteLine($"API Call Failed: {ex.Message}");
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
}