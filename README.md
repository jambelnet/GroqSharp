# GroqSharp

A .NET client library for interacting with Groq's API, providing easy access to their high-performance language models.

![GroqSharp](https://github.com/user-attachments/assets/9d8caca6-d8ac-423e-aaea-fc942a811fed)

## Features

- Simple and intuitive API for chat completions
- Configuration management via `appsettings.json`
- Interactive console interface
- Model management (list available models, change current model)
- Support for all Groq API parameters (temperature, max tokens, etc.)
- Dependency Injection support

## Getting Started

1. Clone or download the GroqSharp repository
2. Add the source files to your .NET project
3. Run the application - it will guide you through initial setup

## Initial Setup

When you first run the application:

1. You'll be prompted to enter your Groq API key (get it from [Groq Console](https://console.groq.com/keys))
2. Set the base URL (defaults to `https://api.groq.com/openai/v1/`)
3. Select a default model from the available options
4. Configure advanced parameters (temperature, max tokens)

This creates an `appsettings.json` file with your configuration for future runs.

## Usage

### Interactive Console

Run the application to enter an interactive chat session:

Groq API Client Commands:
- `/models` - Show available models
- `/setmodel` - Change current model
- `/exit` - Quit the application
- `/help` - Show this help

```text
You: [type your message here]
```

### Programmatic Usage

```csharp
// Setup DI container
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(config);
services.AddHttpClient<IGroqClient, GroqClient>(client =>
{
    client.BaseAddress = new Uri(config["Groq:BaseUrl"]);
});
services.AddTransient<IGroqService, GroqService>();

var serviceProvider = services.BuildServiceProvider();
var groqService = serviceProvider.GetRequiredService<IGroqService>();

// Simple completion
var response = await groqService.GetChatCompletionAsync("Hello, how are you?");

// Advanced completion with parameters
var request = new ChatRequest
{
    Model = "llama-3.3-70b-versatile",
    Messages = new[] { new Message { Role = "user", Content = "Explain quantum computing" } },
    Temperature = 0.7,
    MaxTokens = 1024
};
var advancedResponse = await groqService.GetChatCompletionAsync(request);
```

## Models

List available models with `/models` command or programmatically:

```csharp
var models = await groqService.GetAvailableModelsAsync();
```

Change the current model with `/setmodel` command or by updating the `DefaultModel` in `appsettings.json`.

## Dependencies

- .NET 6.0 or later
 - Microsoft.Extensions.Configuration
 - Microsoft.Extensions.DependencyInjection
 - Microsoft.Extensions.Http

## License

MIT License

## Support

For issues or questions, please open an issue on the GitHub repository.
