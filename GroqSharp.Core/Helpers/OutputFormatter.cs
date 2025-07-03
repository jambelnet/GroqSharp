using System.Text.Json;

namespace GroqSharp.Core.Helpers
{
    public static class OutputFormatter
    {
        /// <summary>
        /// Extracts and formats specific fields from various JSON response formats.
        /// </summary>
        public static string ExtractChatCompletionContent(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Handle transcription-style response
                if (root.TryGetProperty("text", out var textProperty) &&
                    root.TryGetProperty("language", out var languageProperty) &&
                    root.TryGetProperty("duration", out var durationProperty))
                {
                    return $"Language: {languageProperty.GetString()}\n" +
                           $"Duration: {durationProperty.GetDouble()} seconds\n" +
                           $"Text: {textProperty.GetString()?.Trim()}";
                }

                // Original Groq standard chat completion format
                if (root.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentProperty))
                    {
                        return contentProperty.GetString()?.Trim() ?? "(no content)";
                    }
                }

                // Fallback if no known structures found
                if (root.TryGetProperty("text", out var simpleTextProperty))
                {
                    return simpleTextProperty.GetString()?.Trim() ?? "(no text content)";
                }

                // Final fallback - pretty-printed JSON
                return JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
                return "(invalid JSON response)";
            }
            catch (Exception)
            {
                return "(error extracting content)";
            }
        }
    }
}
