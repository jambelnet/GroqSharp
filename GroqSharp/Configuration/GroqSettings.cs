namespace GroqSharp.Configuration
{
    public class GroqSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
        public string DefaultModel { get; set; } = "llama-3.3-70b-versatile";
        public double DefaultTemperature { get; set; }
        public int DefaultMaxTokens { get; set; }
    }
}
