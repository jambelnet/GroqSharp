using GroqSharp.Core.Models;

namespace GroqSharp.Core.Helpers
{
    public static class MessageExtensions
    {
        // Remove executed tools from messages for user-facing API calls
        public static IEnumerable<Message> SanitizeForApi(this IEnumerable<Message> messages)
        {
            return messages.Select(m =>
            {
                var clone = new Message
                {
                    Role = m.Role,
                    Content = m.Content,
                    // Clear executed tools for API input (prevents API errors)
                    ExecutedTools = null
                };
                return clone;
            });
        }

        // Optionally, remove executed tools from a single message if needed
        public static Message SanitizeForDisplay(this Message message)
        {
            return new Message
            {
                Role = message.Role,
                Content = message.Content,
                ExecutedTools = null
            };
        }
    }
}
