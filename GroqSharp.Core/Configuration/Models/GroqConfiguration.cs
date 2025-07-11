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
        public string WhisperLanguage { get; internal set; } = "en";
        public string DefaultReasoningModel { get; set; } = "deepseek-r1-distill-llama-70b";
        public string DefaultAgenticModel { get; set; } = "compound-beta";
        public double DefaultTemperature { get; set; } = 0.7;
        public int DefaultMaxTokens { get; set; } = 1024;
        public double DefaultReasoningTemperature { get; set; } = 0.6;
        public double DefaultVisionTemperature { get; set; } = 1;
        public int DefaultReasoningMaxTokens { get; set; } = 4096;
        public int DefaultTopP { get; set; } = 1;
        public double DefaultReasoningTopP { get;set ; } = 0.95;
    }
}
