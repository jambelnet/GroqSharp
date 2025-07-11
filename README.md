# GroqSharp

GroqSharp is a modern, modular .NET client library with extensible console and Web API frontends for interacting with Groq's LLMs.  
It supports structured prompts, command-based interaction, multimodal capabilities (vision, speech-to-text, text-to-speech), file-based extraction, and HTTP-based chat.

![GroqSharp Vision](https://github.com/user-attachments/assets/1c21a120-b863-45fd-b65a-ed51a962b425)

## Chat UI (Frontend)

The `frontend/` folder contains a standalone HTML/CSS/JS interface for interacting with the GroqSharp WebAPI.

**Dark Mode**

![GroqSharp Dark](https://github.com/user-attachments/assets/5301e27c-c245-421b-870f-9fca90047249)

**Light Mode**

![GroqSharp Light](https://github.com/user-attachments/assets/d57306f5-f8e3-429f-847b-934363a98080)

## Features

- Interactive console with natural and command-based input (CLI)
- HTTP-based Web API for frontend or automation (Web API)
- Vision API support with image URL and base64 input
- Text-to-Speech via `playai-tts` (WAV output, configurable voices)
- Speech-to-Text using `whisper-large-v3-turbo` (verbose JSON)
- Reasoning command support `/reason` for structured step-by-step problem solving with memory
- Agentic command support `/agent` for structured tool invocation with domain/country filters and detailed tool output
- Supports command flags like `--summary` and `--verbose` for controlling output verbosity of executed tools
- Sanitizes message history before sending to API to avoid errors due to tool metadata
- Captures executed tool input and output, saving them as part of conversation history
- Supports domain and country filters in search via extended SearchSettings integration
- API key and model configuration via `appsettings.json`
- File import support (`.pdf`, `.docx`, `.html`) in CLI
- Auto-save and archive chat sessions using in-memory sessions with persistent JSON backing (CLI & Web API)
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
    "DefaultAgenticModel": "compound-beta",
    "DefaultVisionModel": "meta-llama/llama-4-scout-17b-16e-instruct",
    "DefaultTTSModel": "playai-tts",
    "DefaultWhisperModel": "whisper-large-v3-turbo",
    "DefaultTemperature": 0.7,
    "DefaultMaxTokens": 1024,
    "WhisperLanguage": "en"
  }
}
```

## Web API Endpoints

### Chat

- `POST /api/conversations/{sessionId}/chat/messages` — Submit a message to the chat session

### Session Management

- `POST /api/conversations/{sessionId}/new` — Create or load a conversation with optional title

- `POST /api/conversations/{sessionId}/clear` — Clears the conversation history

- `POST /api/conversations/{sessionId}/rename?newTitle=CustomTitle` — Renames a session

- `DELETE /api/conversations/{sessionId}` — Deletes the session file

- `GET /api/conversations/{sessionId}/load` — Returns session title and current memory state

- `GET /api/conversations/{sessionId}/history` — Returns the full conversation history

### Model

- `GET /api/model/list` — List available models

- `POST /api/model/set`  
  ```json
  { "model": "llama-3.3-70b-versatile" }
  ```

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
- `/agent` – Use agentic models with structured tool invocation, supporting domain/country filters and output verbosity control
- `/exit` – Exit the CLI
- `/help` – Display available commands

## Multimodal Capabilities

GroqSharp supports multimodal AI use cases:

| Feature             | Description                                                                                          |
|---------------------|------------------------------------------------------------------------------------------------------|
| **Vision**          | Analyze images using `meta-llama/llama-4-scout-17b-16e-instruct` via file or URL                     |
| **Text-to-Speech**  | Uses `playai-tts` for converting text to WAV audio with configurable voices                          |
| **Speech-to-Text**  | Uses `whisper-large-v3-turbo` for accurate, multilingual transcriptions                              |
| **Translation**     | Uses `whisper-large-v3-turbo` to translate non-English audio to English                                    |
| **Reasoning**       | Uses `deepseek-r1-distill-llama-70b` or `qwen3-32b` for structured thinking                          |
| **Agentic Tooling** | Structured tool invocation using `compound-beta` with support for filters and tool result inspection |

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

The reasoning output will include internal `<think>...</think>` steps when supported by the model.

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

## Agentic Tooling

```text
/agent What is the weather in Paris? --verbose --exclude=example.com --country=France
```

Use the `--exclude` and `--include` flags to filter search domains and the `--country` flag to restrict results by country. The `--verbose` flag shows detailed executed tool information.

**Notes**

- Agentic tooling commands enhance interaction by allowing fine-grained control over search filters and tool execution
- Details of executed tools (name, input, output) are shown in verbose or summary modes to enhance transparency and debugging
- Conversations track the full context, including user queries and tool results, ensuring session persistence and export readiness

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
/export /agent What's the population of Japan? C:\output\agent.txt
```

If no output file path is provided, the CLI will prompt for one.

## Programmatic Usage

```csharp
services.AddGroqSharpCore(configuration);
var service = provider.GetRequiredService<IGroqService>();

var result = await service.GetChatCompletionAsync("Tell me a joke");
```

### Fluent Configuration

```csharp
var request = new ChatRequestBuilder()
    .WithModel("llama-3.3-70b-versatile")
    .WithMessages(new[] { new Message { Role = MessageRole.User, Content = "Tell me a joke" } })
    .WithMaxTokens(1024)
    .Build();

var result = await service.GetChatCompletionAsync(request);
```

## Design Notes

GroqSharp separates conversation persistence (`ConversationSession`) from runtime memory (`ConversationService`). Web API and CLI both create a `ConversationService` per request/session context to ensure clean state management. Sessions are persisted in JSON files and rehydrated on load or message send.

- `ConversationService` holds the runtime chat state and is re-created per session use.
- `ConversationSession` holds the flat, serializable session data: `SessionId`, `Title`, `Model`, and `Messages`.

## License

MIT License

## Support

Open an issue on the [GitHub repo](https://github.com/jambelnet/GroqSharp/issues) for bugs or suggestions.
