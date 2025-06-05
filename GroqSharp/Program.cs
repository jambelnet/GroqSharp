using GroqSharp;
using GroqSharp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // Initial check for required configuration
            if (string.IsNullOrEmpty(config["Groq:ApiKey"]))
            {
                Console.WriteLine("Configuration missing or incomplete. Starting setup...");
                await RunInitialSetup();
                // Reload configuration after creation
                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();
            }

            // Configure services
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);

            // Configure HttpClient with BaseAddress
            services.AddHttpClient<IGroqClient, GroqClient>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            // Register GroqClient with explicit configuration
            services.AddTransient<IGroqClient>(provider =>
            {
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                httpClient.BaseAddress = new Uri(config["Groq:BaseUrl"]);
                return new GroqClient(httpClient, config.GetSection("Groq"));
            });

            services.AddTransient<IGroqService, GroqService>();

            var serviceProvider = services.BuildServiceProvider();
            var groqService = serviceProvider.GetRequiredService<IGroqService>();

            await RunApplication(groqService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }
    }

    private static async Task RunInitialSetup()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.groq.com/openai/v1/") };
        var tempClient = new GroqClient(httpClient, null); // Temporary client for setup

        Console.WriteLine("Enter your Groq API key (get it from https://console.groq.com):");
        var apiKey = GetRequiredInput("API Key: ");

        Console.WriteLine("\nEnter base URL (press Enter for default 'https://api.groq.com/openai/v1/'):");
        var baseUrl = Console.ReadLine();
        baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.groq.com/openai/v1/" : baseUrl;

        Console.WriteLine("\nFetching available models...");
        var availableModels = await tempClient.GetAvailableModelsAsync();

        Console.WriteLine("\nAvailable Models:");
        for (int i = 0; i < availableModels.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {availableModels[i]}");
        }

        Console.Write($"\nSelect model (1-{availableModels.Count}, press Enter for default): ");
        var modelInput = Console.ReadLine();

        var selectedModel = int.TryParse(modelInput, out var num) && num > 0 && num <= availableModels.Count
            ? availableModels[num - 1]
            : "llama-3.3-70b-versatile";

        Console.WriteLine("\nConfigure advanced parameters (press Enter for defaults):");

        Console.Write("Temperature (0-2, default 0.7): ");
        var tempInput = Console.ReadLine();
        double temperature = double.TryParse(tempInput, out var temp) ? Math.Clamp(temp, 0, 2) : 0.7;

        Console.Write("Max Tokens (default 1024): ");
        var maxTokensInput = Console.ReadLine();
        int? maxTokens = int.TryParse(maxTokensInput, out var tokens) ? tokens : 1024;

        // Save to config
        var config = new
        {
            Groq = new
            {
                ApiKey = apiKey,
                BaseUrl = baseUrl,
                DefaultModel = selectedModel,
                DefaultTemperature = temperature,
                DefaultMaxTokens = maxTokens
            }
        };

        await File.WriteAllTextAsync("appsettings.json",
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("\nConfiguration file created successfully!");
    }

    private static async Task RunApplication(IGroqService groqService)
    {
        Console.WriteLine("Groq API Client");
        Console.WriteLine("Commands:");
        Console.WriteLine("  /models - Show available models");
        Console.WriteLine("  /exit   - Quit the application");
        Console.WriteLine("  /help   - Show this help");

        while (true)
        {
            Console.Write("\nYou: ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput)) continue;

            if (userInput.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (userInput.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("  /models - Show available models");
                Console.WriteLine("  /exit   - Quit the application");
                Console.WriteLine("  /help   - Show this help");
                continue;
            }

            if (userInput.Equals("/models", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Console.WriteLine("Fetching available models...");
                    var models = await groqService.GetAvailableModelsAsync();
                    Console.WriteLine("\nAvailable Models:");
                    foreach (var model in models)
                    {
                        Console.WriteLine($"- {model}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching models: {ex.Message}");
                }
                continue;
            }

            try
            {
                Console.Write("AI: ");
                var response = await groqService.GetChatCompletionAsync(userInput);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static string GetRequiredInput(string prompt)
    {
        string input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine()?.Trim();
        } while (string.IsNullOrEmpty(input));

        return input;
    }
}
