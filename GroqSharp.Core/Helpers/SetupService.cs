using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Configuration.Services;
using GroqSharp.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GroqSharp.Core.Helpers
{
    public static class SetupService
    {
        public static async Task RunInitialSetupAsync(string outputFile)
        {
            var tempSettings = new GroqConfiguration
            {
                DefaultModel = ConversationService.DefaultModel
            };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Enter your Groq API key (get it from https://console.groq.com):");
            Console.ResetColor();
            tempSettings.ApiKey = GetRequiredInput("API Key: ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nEnter base URL (press Enter for default 'https://api.groq.com/openai/v1/'): ");
            Console.ResetColor();
            var baseUrl = Console.ReadLine();
            tempSettings.BaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.groq.com/openai/v1/" : baseUrl;

            // Temporary config to fetch models
            var configDict = new Dictionary<string, string>
            {
                { "Groq:ApiKey", tempSettings.ApiKey },
                { "Groq:BaseUrl", tempSettings.BaseUrl },
                { "Groq:DefaultModel", tempSettings.DefaultModel },
                { "Groq:DefaultTemperature", tempSettings.DefaultTemperature.ToString() },
                { "Groq:DefaultMaxTokens", tempSettings.DefaultMaxTokens.ToString() }
            };

            var tempConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            var configService = new GroqConfigurationService(tempConfig);
            var httpClient = new HttpClient { BaseAddress = new Uri(tempSettings.BaseUrl) };
            var tempClient = new GroqClient(httpClient, configService);

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
                : ConversationService.DefaultModel;

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
                    ApiKey = tempSettings.ApiKey,
                    BaseUrl = tempSettings.BaseUrl,
                    DefaultModel = selectedModel,
                    DefaultTemperature = temperature,
                    DefaultMaxTokens = maxTokens
                }
            };

            await File.WriteAllTextAsync(outputFile,
                JsonSerializer.Serialize(finalConfig, new JsonSerializerOptions { WriteIndented = true }));

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
