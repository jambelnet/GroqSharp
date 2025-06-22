namespace GroqSharp.Core.Configuration.Models
{
    public class GroqConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
        public string DefaultModel { get; set; } = "llama-3.3-70b-versatile";
        public string DefaultVisionModel { get; set; } = "meta-llama/llama-4-scout-17b-16e-instruct";
        public string DefaultTTSModel { get; set; } = "playai-tts";
        public string DefaultWhisperModel { get; set; } = "whisper-large-v3-turbo"; // Used for transcription. For translation, use "whisper-large-v3" (English-only output)
        public double DefaultTemperature { get; set; } = 0.7;
        public int DefaultMaxTokens { get; set; } = 1024;
        public string? WhisperLanguage { get; internal set; } = "en";
    }
}
