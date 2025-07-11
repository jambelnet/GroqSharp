namespace GroqSharp.Core.Models
{
    public class RequestDefaults
    {
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
        public double TopP { get; set; }
        public bool Stream { get; set; }
    }
}
