using GroqSharp;
using GroqSharp.Commands.Models;
using GroqSharp.Configuration;
using GroqSharp.Core;
using GroqSharp.Extensions;
using GroqSharp.Models;
using GroqSharp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build and validate configuration
            var config = await InitializeConfigurationAsync();

            // Setup DI container
            var services = new ServiceCollection();
            ConfigureServices(services, config);

            var serviceProvider = services.BuildServiceProvider();

            // Run the interactive console app
            await RunApplication(serviceProvider);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static async Task<IConfiguration> InitializeConfigurationAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        if (string.IsNullOrWhiteSpace(config["Groq:ApiKey"]))
        {
            Console.WriteLine("Configuration missing or incomplete. Starting setup...");
            await RunInitialSetup();

            // Reload configuration
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        return config;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // Configuration
        services.AddSingleton(config);
        services.Configure<GroqSettings>(config.GetSection("Groq"));

        // Conversation persistence
        services.AddSingleton<ConversationPersistenceService>();
        services.AddScoped<CommandContext>();

        // Core Groq services
        services.AddHttpClient<IGroqClient, GroqClient>(client =>
        {
            client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
        });
        services.AddTransient<IGroqService, GroqService>();

        // Commands
        services.AddGroqSharpCommands();
    }

    private static async Task RunInitialSetup()
    {
        var tempSettings = new GroqSettings
        {
            DefaultModel = GroqConstants.DefaultModel
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

        var httpClient = new HttpClient { BaseAddress = new Uri(tempSettings.BaseUrl) };
        var tempClient = new GroqClient(httpClient, Options.Create(tempSettings));

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
            : GroqConstants.DefaultModel;

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

        var config = new
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

        await File.WriteAllTextAsync("appsettings.json",
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nConfiguration file created successfully!");
        Console.ResetColor();
    }

    private static async Task RunApplication(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        // Get all required services from the scoped provider
        var groqService = scopedProvider.GetRequiredService<IGroqService>();
        var dispatcher = scopedProvider.GetRequiredService<CommandDispatcher>();
        var context = scopedProvider.GetRequiredService<CommandContext>();

        try
        {
            // Initialize context properties
            context.GroqService = groqService;
            context.CurrentModel = scopedProvider.GetRequiredService<IConfiguration>()["Groq:DefaultModel"];

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Groq API Client");
            Console.WriteLine("Available commands: " + string.Join(", ", dispatcher.GetAllCommands()));
            Console.ResetColor();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nYou: ");
                Console.ResetColor();
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.StartsWith("/"))
                {
                    await dispatcher.Dispatch(userInput, context);
                    if (context.ShouldExit) break;
                }
                else
                {
                    await ProcessAiInput(userInput, context);
                }
            }
        }
        finally
        {
            context.SaveConversation(); // Auto-save when session ends
        }
    }

    private static async Task ProcessAiInput(string input, CommandContext context)
    {
        try
        {
            // Add user message to history
            context.Conversation.AddMessage("user", input);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("AI: ");
            Console.ResetColor();

            var request = new ChatRequest
            {
                Model = context.CurrentModel ?? GroqConstants.DefaultModel,
                Messages = context.Conversation.GetApiMessages(),
                Temperature = 0.7
            };

            string response = await context.GroqService.GetChatCompletionAsync(request);

            // Add AI response to history
            context.Conversation.AddMessage("assistant", response);

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
