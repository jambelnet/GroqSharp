namespace GroqSharp.Core.Configuration.Models
{
    public class GroqConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
        public string DefaultModel { get; set; } = "llama-3.3-70b-versatile";
        public double DefaultTemperature { get; set; } = 0.7;
        public int DefaultMaxTokens { get; set; } = 1024;
    }
}
