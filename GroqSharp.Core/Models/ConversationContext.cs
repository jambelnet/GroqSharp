namespace GroqSharp.Core.Models
{
    public class ConversationContext
    {
        public List<Message> Messages { get; set; } = new();

        public void ClearHistory() => Messages.Clear();
    }
}
