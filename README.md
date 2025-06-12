# GroqSharp

GroqSharp is a modern, extensible .NET console application and SDK for interacting with Groq's LLMs. It supports structured prompts, command-based interaction, and file-based text extraction, all wrapped in a configurable and pluggable architecture.


![GroqSharp](https://github.com/user-attachments/assets/9d8caca6-d8ac-423e-aaea-fc942a811fed)

## Features

- Interactive console with natural and command-based input
- API key and model configuration via `appsettings.json`
- File import support (`.pdf`, `.docx`, `.html`)
- Dependency Injection ready
- Chat history persistence
- Export conversations to `.txt`
- Switch models dynamically
- Streaming and non-streaming chat modes
- Slash commands for model management, help, and more

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

## Configuration

First-run setup creates `appsettings.json` with:

```json
json
{
  "Groq": {
    "ApiKey": "your_api_key",
    "BaseUrl": "https://api.groq.com/openai/v1/",
    "DefaultModel": "llama-3.3-70b-versatile",
    "DefaultTemperature": 0.7,
    "DefaultMaxTokens": 1024
  }
}
```

## Usage

### Interactive Console

Run the application to enter an interactive chat session:

Groq API Client Commands:
`/models`   - Show available models
`/setmodel` - Change current model
`/stream`   - Start a streaming chat session
`/history`  - Show previous conversation messages
`/clear`    - Clear the current session
`/process`  - Process a local file
`/export`   - Export AI output to file
`/exit`     - Quit the application
`/help`     - Show this help

```text
You: [type your message here]
```

### Programmatic Usage

You can use GroqSharp in your own .NET applications as a library.

### Register services

```csharp
var services = new ServiceCollection();
services.AddGroqSharp(configuration); // configuration is an IConfiguration instance
var provider = services.BuildServiceProvider();

var groqService = provider.GetRequiredService<IGroqService>();
```

### Send a Chat Request

```csharp
var response = await groqService.CompleteChatAsync(new ChatRequest
{
    Messages = new List<Message>
    {
        new Message { Role = "user", Content = "Tell me a joke." }
    },
    Stream = false // or true for streaming
});

Console.WriteLine(response?.Choices?.FirstOrDefault()?.Message?.Content);
```

### List Available Models

```csharp
var models = await groqService.GetAvailableModelsAsync();
foreach (var model in models)
{
    Console.WriteLine(model.Id);
}
```

## Models

List available models with `/models` command or programmatically:

```csharp
var models = await groqService.GetAvailableModelsAsync();
```

Change the current model with `/setmodel` command or by updating the `DefaultModel` in `appsettings.json`.

## Stream

Start a streaming session:

```text
/stream
```

Type your message:

```text
You: Explain quantum computing in simple terms
```

See real-time response:

```text
AI: Quantum computing is like a super-powered computer that uses quantum bits...
(text appears chunk by chunk)
```

End streaming:

```text
/end
```

## File Processing

Supported file formats:

- Text files (.txt)
- Office documents (.docx)
- PDF files (.pdf)
- HTML files (.html)

Example:

```bash
/process sample.pdf
Process this content? (y/n): y
Export this response? (y/n): y
Enter output path: summary.txt
```

## Export

AI output to export:

```bash
/export This is a sample request
Enter output path (default extension .txt): C:\Temp\sample.txt
Successfully exported to C:\Temp\sample.txt
```

## Dependencies

- .NET 6.0 or later
 - Microsoft.Extensions.Configuration
 - Microsoft.Extensions.DependencyInjection
 - Microsoft.Extensions.Http

## License

MIT License

## Support

For issues or questions, please open an issue on the GitHub repository.
