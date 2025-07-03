using System.Text.Json.Serialization;

namespace GroqSharp.Core.Models
{
    public class SearchSettings
    {
        [JsonPropertyName("exclude_domains")]
        public string[]? ExcludeDomains { get; set; }

        [JsonPropertyName("include_domains")]
        public string[]? IncludeDomains { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
}
