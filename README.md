# GroqSharp

GroqSharp is a modern, modular .NET client library with extensible console and Web API frontends for interacting with Groq's LLMs.
It supports structured prompts, command-based interaction, file-based text extraction, and HTTP-based chat — all built on a pluggable, provider-agnostic architecture.

![GroqSharp](https://github.com/user-attachments/assets/9d8caca6-d8ac-423e-aaea-fc942a811fed)

## Features

- Interactive console with natural and command-based input (CLI)
- HTTP-based Web API for headless or frontend integration (Web API)
- API key and model configuration via `appsettings.json`
- File import support (`.pdf`, `.docx`, `.html`) (CLI)
- Conversation history autosave and archive (CLI & Web API)
- Export conversations (CLI)
- Switch models dynamically
- Slash commands (CLI) and REST endpoints (Web API)
- Dependency Injection and modular architecture

## Getting Started

### CLI Setup

1. Clone the repository
2. Run the console project (`GroqSharp.CLI`)
3. Follow the guided setup to configure your Groq API key and model

### Web API Setup

1. Start `GroqSharp.WebAPI` project
2. Use tools like [Postman](https://www.postman.com/) or [curl](https://curl.se/) to send HTTP requests
3. No extra configuration is needed if `appsettings.json` is pre-generated from CLI or manually created

## Initial Setup (CLI)

When you first run the CLI:

1. You'll be prompted to enter your Groq API key (get it from [Groq Console](https://console.groq.com/keys))
2. Optionally set base URL (defaults to `https://api.groq.com/openai/v1/`)
3. Select a default model from the available options
4. Configure advanced parameters (temperature, max tokens)

This creates an `appsettings.json` file for future runs.

## Configuration

First-run setup creates `appsettings.json` with:

```json
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

## Web API Endpoints

`POST /api/conversations/{sessionId}/chat/messages`

```json
{
  "role": "user",
  "content": "What's the capital of Japan?"
}
```

```json
{
  "response": "The capital of Japan is Tokyo."
}
```

`GET /api/conversations/{sessionId}/history`

Returns full chat history for a session.

`DELETE /api/conversations/{sessionId}`

Deletes an archived session (if saved).

`POST /api/model/set`

```json
{
  "model": "llama-3.3-70b-versatile"
}
```

`GET /api/model/list`

Returns available models from Groq API.

## CLI Commands

Launch the CLI app and use these commands:

**GroqSharp CLI Commands:**

- `/models`    - Show available models
- `/setmodel`  - Change current model
- `/stream`    - Start streaming chat
- `/history`   - Show conversation history
- `/clear`     - Clear session memory
- `/archive`   - List/load/delete/rename conversations
- `/new`       - Start a new chat session
- `/process`   - Import and analyze a file
- `/export`    - Save AI output to a file
- `/exit`      - Quit the application
- `/help`      - Show help

```text
You: [type your message here]
```

## Programmatic Usage (Library)

### Register services

```csharp
var services = new ServiceCollection();
services.AddGroqSharp(configuration); // IConfiguration instance
var provider = services.BuildServiceProvider();

var groqService = provider.GetRequiredService<IGroqService>();
```

### Chat Request Example

```csharp
var response = await groqService.CompleteChatAsync(new ChatRequest
{
    Messages = new List<Message>
    {
        new Message { Role = "user", Content = "Tell me a joke." }
    }
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

## Conversation Archives

GroqSharp automatically saves your chat sessions as JSON files in the background. These are stored in the following folder:

`%APPDATA%\GroqSharp\global_conversations\`

💡 Archives are saved under `%APPDATA%\GroqSharp\global_conversations\` on Windows.
On Linux/macOS, it's typically `~/.config/GroqSharp/global_conversations/`.

Each file is named using the session ID, for example:

`3fba2d5e-7a2a-4f48-8c29-84692e17f289.json`

### Archive Metadata

GroqSharp maintains a title and a preview for each conversation. Titles can be renamed, and previews help identify the content at a glance. The archive list shows:

- Session ID
- Title
- Last modified timestamp
- First line preview

### Managing Archives via CLI

Use the `/archive` command followed by one of the subcommands:

| Command                             | Description                         |
| ----------------------------------- | ----------------------------------- |
| `/archive list`                     | List all saved conversations        |
| `/archive load [sessionId]`         | Load a conversation by ID or index  |
| `/archive delete [sessionId]`       | Delete a saved conversation         |
| `/archive rename [sessionId] title` | Rename the archive with a new title |

Use the full session ID or the number shown in the list.

Example:

```text
/archive list
1. [3fba2d5e-7a2a-4f48-8c29-84692e17f289] Write a song
   Last Modified: 2025-06-18
   Preview: Write a dystopian-themed punk rock chorus...

/archive rename 3fba2d5e-7a2a-4f48-8c29-84692e17f289 "Dystopian Punk Session"
/archive delete 3fba2d5e-7a2a-4f48-8c29-84692e17f289
```

## Starting a New Chat

Use `/new` to start a fresh chat session at any time:

```text
/new
```

GroqSharp will automatically save the current conversation before resetting the session. This keeps your archive organized without asking for confirmation.

## File Processing

Supported file formats:

- Text files (.txt)
- Office documents (.docx)
- PDF files (.pdf)
- HTML files (.html)

Example:

```text
/process sample.pdf
Process this content? (y/n): y
Export this response? (y/n): y
Enter output path: summary.txt
```

## Export

AI output to export:

```text
/export This is a sample request
Enter output path (default extension .txt): C:\Temp\sample.txt
Successfully exported to C:\Temp\sample.txt
```

## Dependencies

- .NET 8.0 or later
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Http

## License

MIT License

## Support

For issues or questions, please open an issue on the GitHub repository.
