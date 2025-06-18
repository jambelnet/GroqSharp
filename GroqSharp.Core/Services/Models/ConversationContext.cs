using GroqSharp.Core.Models;

namespace GroqSharp.Core.Services.Models
{
    public class ConversationContext
    {
        public List<Message> Messages { get; set; } = new();

        public void ClearHistory() => Messages.Clear();
    }
}
