# GroqSharp

GroqSharp is a modern, modular .NET client library with extensible console and Web API frontends for interacting with Groq's LLMs.  
It supports structured prompts, command-based interaction, multimodal capabilities (vision, speech-to-text, text-to-speech), file-based extraction, and HTTP-based chat.

![GroqSharp Vision](https://github.com/user-attachments/assets/1c21a120-b863-45fd-b65a-ed51a962b425)

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
- `/new` – Start a new chat session
- `/history` – View conversation history
- `/clear` – Clear current session memory
- `/archive` – Manage saved conversations
- `/process` – Import and analyze a file
- `/export` – Save AI output to file
- `/speak` – Convert text to speech (WAV)
- `/transcribe` – Transcribe audio file to text
- `/translate` – Translate non-English audio to English
- `/vision` – Analyze an image (URL or file)
- `/reason` - Solve problems step-by-step using structured reasoning with memory
- `/exit` – Exit the CLI
- `/help` – Display available commands

## Multimodal Capabilities

GroqSharp supports multimodal AI use cases:

| Feature            | Description                                                                      |
|--------------------|----------------------------------------------------------------------------------|
| **Vision**         | Analyze images using `meta-llama/llama-4-scout-17b-16e-instruct` via file or URL |
| **Text-to-Speech** | Uses `playai-tts` for converting text to WAV audio with configurable voices      |
| **Speech-to-Text** | Uses `whisper-large-v3-turbo` for accurate, multilingual transcriptions          |
| **Translation**    | Uses `whisper-large-v3` to translate non-English audio to English                |
| **Reasoning**      | Uses `deepseek-r1-distill-llama-70b` or `qwen3-32b` for structured thinking      |

### Multimodal Usage Examples

#### Vision

```text
/vision https://example.com/image.jpg "Describe this scene"
/export /vision image.jpg "What do you see?" C:\output\vision.txt
```

#### Text-to-Speech

```text
/speak "Welcome to GroqSharp!"
/export /speak "Hello world!" C:\output\speech.wav
```

#### Speech-to-Text

```text
/transcribe C:\path\to\audio.mp3
```

#### Translation

```text
/translate
Enter audio file path to translate: C:\path\foreign_audio.mp3
Translation result: "Welcome to our program. Today we’ll discuss..."
```

## Streaming Chat

Use `/stream` to start streaming output. End with `/end`.

```text
You: What is quantum computing?
AI: Quantum computing is a technology that...
```

## Reasoning

Use `/reason` to invoke a structured reasoning task with memory persistence:

```text
/reason What's the capital of Luxembourg?
```

Or specify new problems mid-session:

```text
/reason Write a Java Hello World program
/reason Convert it to C#
/reason Now generate a Python version
```

The reasoning output will include internal <think>...</think> steps when supported by the model.

**Example Output**

```text
You: /reason Write a Hello World in Python

Reasoning Output:
<think>
Okay, I need to write a simple Python program that prints 'Hello, World!'.
I'll use the print function...
</think>

print("Hello, World!")
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

You can export AI responses or the results of other CLI commands.

```text
/export "This is a sample summary" C:\output\summary.txt
/export /vision https://example.com/image.jpg "What do you see?" C:\output\vision.txt
/export /speak "Hello world!" C:\output\speech.wav
```

If no output file path is provided, the CLI will prompt for one.

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
