using System.Text.Json.Serialization;

namespace GroqSharp.Core.Services.Models
{
    public class ConversationSession
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public ConversationService Conversation { get; set; }
        public DateTime LastModified { get; set; }

        [JsonIgnore]
        public string DisplayName => $"{Title} ({SessionId[..8]})";
    }
}
