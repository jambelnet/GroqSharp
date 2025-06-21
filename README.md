# GroqSharp

GroqSharp is a modern, modular .NET client library with extensible console and Web API frontends for interacting with Groq's LLMs.  
It supports structured prompts, command-based interaction, multimodal capabilities (vision, speech-to-text, text-to-speech), file-based extraction, and HTTP-based chat.

![GroqSharp](https://github.com/user-attachments/assets/9d8caca6-d8ac-423e-aaea-fc942a811fed)

## Features

- Interactive console with natural and command-based input (CLI)
- HTTP-based Web API for frontend or automation (Web API)
- Vision API support with image URL and base64 input
- Text-to-Speech via `playai-tts` (WAV output, configurable voices)
- Speech-to-Text using `whisper-large-v3-turbo` (verbose JSON)
- API key and model configuration via `appsettings.json`
- File import support (`.pdf`, `.docx`, `.html`) in CLI
- Auto-save and archive chat sessions (CLI & Web API)
- Conversation export and title/preview management
- Model switching and listing (CLI & Web API)
- Slash commands and REST endpoints
- Fully modular architecture with dependency injection

## Getting Started

### CLI Setup

1. Clone the repository
2. Run the `GroqSharp.CLI` project
3. Follow the interactive setup to configure your Groq API key and model

### Web API Setup

1. Start the `GroqSharp.WebAPI` project
2. Use [Postman](https://www.postman.com/) or `curl` to test endpoints
3. `appsettings.json` is shared from CLI or can be configured manually

## Initial Setup (CLI)

Upon first launch:

1. You'll be prompted for your Groq API key ([get yours here](https://console.groq.com/keys))
2. Choose a default model (auto-fetches from Groq API)
3. Configure additional parameters (temperature, max tokens, etc.)
4. `appsettings.json` is generated

## Configuration

Sample `appsettings.json`:

```json
{
  "Groq": {
    "ApiKey": "your_api_key",
    "BaseUrl": "https://api.groq.com/openai/v1/",
    "DefaultModel": "llama-3.3-70b-versatile",
    "DefaultTemperature": 0.7,
    "DefaultMaxTokens": 1024,
    "DefaultVisionModel": "meta-llama/llama-4-scout-17b-16e-instruct",
    "DefaultTTSModel": "playai-tts",
    "DefaultWhisperModel": "whisper-large-v3-turbo",
    "WhisperLanguage": "en"
  }
}
```

## Web API Endpoints

- `POST /api/conversations/{sessionId}/chat/messages`  
  Submit a message to the chat session

- `GET /api/conversations/{sessionId}/history`  
  Fetch chat history

- `DELETE /api/conversations/{sessionId}`  
  Delete a saved session

- `POST /api/model/set`  
  ```json
  { "model": "llama-3.3-70b-versatile" }
  ```

- `GET /api/model/list`  
  List available models

## CLI Slash Commands

- `/models` – List all available models
- `/setmodel` – Change the active model
- `/stream` – Stream chat response
- `/history` – View conversation history
- `/clear` – Clear current session memory
- `/archive` – Manage saved conversations
- `/new` – Start a new chat session
- `/process` – Import and analyze a file
- `/export` – Save AI output to file
- `/vision` – Analyze an image (URL or file)
- `/speak` – Convert text to speech (WAV)
- `/transcribe` – Transcribe audio file to text
- `/exit` – Exit the CLI
- `/help` – Display available commands

## Multimodal Capabilities

GroqSharp supports multimodal AI use cases:

| Feature            | Description                                                                      |
|--------------------|----------------------------------------------------------------------------------|
| **Vision**         | Analyze images using `meta-llama/llama-4-scout-17b-16e-instruct` via file or URL |
| **Text-to-Speech** | Uses `playai-tts` for converting text to WAV audio with configurable voices      |
| **Speech-to-Text** | Uses `whisper-large-v3-turbo` for accurate, multilingual transcriptions          |

## Streaming Chat

Use `/stream` to start streaming output. End with `/end`.

```text
You: What is quantum computing?
AI: Quantum computing is a technology that...
```

## Archive Management

Archives are saved to:

- Windows: `%APPDATA%\GroqSharp\global_conversations\`
- Linux/macOS: `~/.config/GroqSharp/global_conversations/`

Use `/archive list`, `/archive load`, `/archive rename`, `/archive delete` for managing them.

## File Processing

Import `.txt`, `.pdf`, `.docx`, or `.html` files using:

```text
/process report.pdf
```

AI will analyze the content, and you can export output after.

## Exporting Output

```text
/export This is a sample summary
Enter path: summary.txt
=> Successfully exported
```

## Programmatic Usage

```csharp
services.AddGroqSharp(configuration);
var service = provider.GetRequiredService<IGroqService>();

var result = await service.CompleteChatAsync("Tell me a joke");
```

## License

MIT License

## Support

Open an issue on the [GitHub repo](https://github.com/jambelnet/GroqSharp/issues) for bugs or suggestions.