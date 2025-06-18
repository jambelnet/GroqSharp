using GroqSharp.Core.Models;

namespace GroqSharp.Core.Services.Models
{
    /// <summary>
    /// Represents the serialized format of conversation data stored on disk
    /// </summary>
    public class ConversationData
    {
        public string Title { get; set; } = "New Conversation";
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
