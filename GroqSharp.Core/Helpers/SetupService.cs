using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Configuration.Services;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GroqSharp.Core.Helpers
{
    public static class SetupService
    {
        public static async Task RunInitialSetupAsync(string outputFile)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Enter your Groq API key (get it from https://console.groq.com):");
            Console.ResetColor();
            var apiKey = GetRequiredInput("API Key: ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nEnter base URL (press Enter for default 'https://api.groq.com/openai/v1/'): ");
            Console.ResetColor();
            var baseUrl = Console.ReadLine();
            baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.groq.com/openai/v1/" : baseUrl;

            // Temporary config to fetch models
            var configDict = new Dictionary<string, string>
            {
                { "Groq:ApiKey", apiKey },
                { "Groq:BaseUrl", baseUrl },
                { "Groq:DefaultModel", "llama-3.3-70b-versatile" }, // temp value for resolver
                { "Groq:DefaultTemperature", "0.7" },
                { "Groq:DefaultMaxTokens", "1024" }
            };

            var tempConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            var configService = new GroqConfigurationService(tempConfig);
            var modelResolver = new ModelResolver(tempConfig);
            var defaultModel = modelResolver.GetModelFor(GroqFeature.Default);

            var tempSettings = new GroqConfiguration
            {
                ApiKey = apiKey,
                BaseUrl = baseUrl,
                DefaultModel = defaultModel
            };

            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            var tempClient = new GroqClient(httpClient, configService, modelResolver);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nFetching available models...");
            Console.ResetColor();

            var availableModels = await tempClient.GetAvailableModelsAsync();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nAvailable Models:");
            for (int i = 0; i < availableModels.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{i + 1}. {availableModels[i]}");
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\nSelect model (1-{availableModels.Count}, press Enter for default): ");
            Console.ResetColor();
            var modelInput = Console.ReadLine();

            var selectedModel = int.TryParse(modelInput, out var num) && num > 0 && num <= availableModels.Count
                ? availableModels[num - 1]
                : defaultModel;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nConfigure advanced parameters (press Enter for defaults):");

            Console.Write("Temperature (0–2, default 0.7): ");
            Console.ResetColor();
            var tempInput = Console.ReadLine();
            double temperature = double.TryParse(tempInput, out var tempVal) ? Math.Clamp(tempVal, 0, 2) : 0.7;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Max Tokens (default 1024): ");
            Console.ResetColor();
            var maxTokensInput = Console.ReadLine();
            int maxTokens = int.TryParse(maxTokensInput, out var tokens) ? tokens : 1024;

            var finalConfig = new
            {
                Groq = new
                {
                    ApiKey = apiKey,
                    BaseUrl = baseUrl,
                    DefaultModel = selectedModel,
                    DefaultVisionModel = "meta-llama/llama-4-scout-17b-16e-instruct",
                    DefaultTTSModel = "playai-tts",
                    DefaultWhisperModel = "whisper-large-v3-turbo",
                    WhisperLanguage = "en",
                    DefaultReasoningModel = "deepseek-r1-distill-llama-70b",
                    DefaultAgenticModel = "compound-beta",
                    DefaultTemperature = temperature,
                    DefaultMaxTokens = maxTokens,
                    DefaultReasoningTemperature = 0.6,
                    DefaultVisionTemperature = 1,
                    DefaultReasoningMaxTokens = 4096,
                    DefaultTopP = 1,
                    DefaultReasoningTopP = 0.95
                }
            };
            await File.WriteAllTextAsync(outputFile,
                JsonSerializer.Serialize(finalConfig, JsonDefaults.WriteIndented));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nConfiguration file created successfully!");
            Console.ResetColor();
        }

        private static string GetRequiredInput(string label)
        {
            string? input;
            do
            {
                Console.Write(label);
                input = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(input));

            return input;
        }
    }
}
