using System.Text.Json.Serialization;
using GroqSharp.Core.Services;

namespace GroqSharp.Core.Models
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
