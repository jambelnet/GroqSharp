namespace GroqSharp.Core.Services.Models
{
    public class ConversationMeta
    {
        public string SessionId { get; set; }
        public string Title { get; set; }
        public DateTime LastModified { get; set; }
        public string Preview { get; set; }
    }
}