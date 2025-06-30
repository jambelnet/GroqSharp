using GroqSharp.Core.Services;

namespace GroqSharp.Core.Models
{
    /// <summary>
    /// Represents the serialized format of conversation data stored on disk
    /// </summary>
    public class ConversationData
    {
        public string Title { get; set; } = "New Conversation";
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public List<Message> Messages { get; set; } = new List<Message>();
        public string Model { get; set; } = ConversationService.DefaultModel;
    }
}
