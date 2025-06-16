using GroqSharp.Models;

namespace GroqSharp.Services
{
    public class ConversationService
    {
        private readonly List<Message> _messages = new();
        private readonly int _maxHistoryLength;
        private readonly object _lock = new();

        public const string DefaultModel = "llama-3.3-70b-versatile";
        public string CurrentModel { get; set; } = DefaultModel;

        public ConversationService(int maxHistoryLength = 100)
        {
            _maxHistoryLength = maxHistoryLength;
        }

        public void AddMessage(string role, string content)
        {
            lock (_lock)
            {
                _messages.Add(new Message { Role = role, Content = content });

                // Trim oldest messages if over limit
                while (_messages.Count > _maxHistoryLength)
                {
                    _messages.RemoveAt(0);
                }
            }
        }

        public Message[] GetApiMessages()
        {
            return _messages.ToArray();
        }

        public void LoadMessages(IEnumerable<Message> messages)
        {
            _messages.Clear();
            _messages.AddRange(messages);
        }

        public IEnumerable<Message> GetHistory()
        {
            return _messages.AsReadOnly();
        }

        public IReadOnlyList<Message> GetFullHistory() => _messages.AsReadOnly();

        public void ClearHistory() => _messages.Clear();
    }
}
