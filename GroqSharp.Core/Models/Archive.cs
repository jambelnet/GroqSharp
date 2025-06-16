using System.Text.Json.Serialization;

namespace GroqSharp.Models
{
    public class Archive
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("preview")]
        public string Preview { get; set; }
    }
}
