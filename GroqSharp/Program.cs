using GroqSharp;
using GroqSharp.Models;
using GroqSharp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build configuration from appsettings.json if exists
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // If API key is missing, run initial setup
            if (string.IsNullOrWhiteSpace(config["Groq:ApiKey"]))
            {
                Console.WriteLine("Configuration missing or incomplete. Starting setup...");
                await RunInitialSetup();

                // Reload configuration after creation
                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();
            }

            // Setup DI container
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);

            // Setup HttpClient for GroqClient with BaseAddress from config
            services.AddHttpClient<IGroqClient, GroqClient>(client =>
            {
                client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
            });

            // Register GroqClient explicitly to pass config section as well
            services.AddTransient<IGroqClient>(provider =>
            {
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                httpClient.BaseAddress = new Uri(config["Groq:BaseUrl"]);
                return new GroqClient(httpClient, config.GetSection("Groq"));
            });

            services.AddTransient<IGroqService, GroqService>();

            var serviceProvider = services.BuildServiceProvider();
            var groqService = serviceProvider.GetRequiredService<IGroqService>();

            // Run the interactive console app
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
        var tempClient = new GroqClient(httpClient, null);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Enter your Groq API key (get it from https://console.groq.com):");
        Console.ResetColor();
        var apiKey = GetRequiredInput("API Key: ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nEnter base URL (press Enter for default 'https://api.groq.com/openai/v1/'):");
        Console.ResetColor();
        var baseUrl = Console.ReadLine();
        baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.groq.com/openai/v1/" : baseUrl;

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
            : "llama-3.3-70b-versatile";

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nConfigure advanced parameters (press Enter for defaults):");

        Console.Write("Temperature (0-2, default 0.7): ");
        Console.ResetColor();
        var tempInput = Console.ReadLine();
        double temperature = double.TryParse(tempInput, out var temp) ? Math.Clamp(temp, 0, 2) : 0.7;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Max Tokens (default 1024): ");
        Console.ResetColor();
        var maxTokensInput = Console.ReadLine();
        int maxTokens = int.TryParse(maxTokensInput, out var tokens) ? tokens : 1024;

        // Save to config (no color change needed here)
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

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nConfiguration file created successfully!");
        Console.ResetColor();
    }

    private static async Task RunApplication(IGroqService groqService)
    {
        string currentModel = null; // keep track of chosen model in runtime

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Groq API Client");
        Console.WriteLine("Commands:");
        Console.WriteLine("  /models   - Show available models");
        Console.WriteLine("  /setmodel - Change current model");
        Console.WriteLine("  /exit     - Quit the application");
        Console.WriteLine("  /help     - Show this help");
        Console.ResetColor();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\nYou: ");
            Console.ResetColor();
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            if (userInput.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (userInput.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Available commands:");
                Console.WriteLine("  /models   - Show available models");
                Console.WriteLine("  /setmodel - Change current model");
                Console.WriteLine("  /exit     - Quit the application");
                Console.WriteLine("  /help     - Show this help");
                Console.ResetColor();
                continue;
            }

            if (userInput.Equals("/models", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Fetching available models...");
                    Console.ResetColor();

                    var models = await groqService.GetAvailableModelsAsync();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\nAvailable Models:");
                    foreach (var model in models)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"- {model}");
                    }
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error fetching models: {ex.Message}");
                    Console.ResetColor();
                }
                continue;
            }

            if (userInput.Equals("/setmodel", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var models = await groqService.GetAvailableModelsAsync();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\nAvailable Models:");
                    for (int i = 0; i < models.Count; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"{i + 1}. {models[i]}");
                    }
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Select model (1-{models.Count}): ");
                    Console.ResetColor();
                    var input = Console.ReadLine();

                    if (int.TryParse(input, out var choice) && choice > 0 && choice <= models.Count)
                    {
                        currentModel = models[choice - 1];
                        await UpdateModelInAppSettingsAsync(currentModel);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Model changed to '{currentModel}'.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid selection. Model not changed.");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error setting model: {ex.Message}");
                    Console.ResetColor();
                }
                continue;
            }

            // Now, when calling the chat completion, pass the selected model if possible.
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("AI: ");
                Console.ResetColor();

                // Create a ChatRequest with the selected model, or just use the string method if none selected
                string response;
                if (currentModel != null)
                {
                    var chatRequest = new ChatRequest
                    {
                        Model = currentModel,
                        Messages = new[]
                        {
                            new Message { Role = "user", Content = userInput }
                        }
                    };

                    response = await groqService.GetChatCompletionAsync(chatRequest);
                }
                else
                {
                    response = await groqService.GetChatCompletionAsync(userInput);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(response);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
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

    static async Task UpdateModelInAppSettingsAsync(string newModel)
    {
        var filePath = "appsettings.json";

        if (!File.Exists(filePath))
        {
            Console.WriteLine("appsettings.json file not found.");
            return;
        }

        var jsonText = await File.ReadAllTextAsync(filePath);
        var jsonNode = JsonNode.Parse(jsonText);

        if (jsonNode == null)
        {
            Console.WriteLine("Failed to parse appsettings.json.");
            return;
        }

        // Navigate to Groq section
        var groqSection = jsonNode["Groq"]?.AsObject();

        if (groqSection == null)
        {
            Console.WriteLine("Groq section not found in appsettings.json.");
            return;
        }

        // Update the model value
        groqSection["DefaultModel"] = newModel;

        // Write back updated JSON
        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(filePath, jsonNode.ToJsonString(options));

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Model updated to '{newModel}' in appsettings.json.");
        Console.ResetColor();
    }
}
