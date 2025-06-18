using GroqSharp.Core.Models;
using GroqSharp.Core.Services.Interfaces;

namespace GroqSharp.Core.Services
{
    public class ConversationService : IAutoSaveConversation
    {
        private readonly object _lock = new(); 
        private readonly int _maxHistoryLength;
        private string _sessionId;
        private readonly IGlobalConversationService _globalConversationService;
        private readonly List<Message> _messages = new();

        public const string DefaultModel = "llama-3.3-70b-versatile";
        public string CurrentModel { get; set; } = DefaultModel;

        public ConversationService(
            int maxHistoryLength = 100)
        {
            _maxHistoryLength = maxHistoryLength;
        }

        public void Initialize(string sessionId)
        {
            _sessionId = sessionId;
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

            // Fire-and-forget auto-save
            _ = TryAutoSave();
        }

        private async Task TryAutoSave()
        {
            try
            {
                if (!string.IsNullOrEmpty(_sessionId))
                {
                    await _globalConversationService.SaveSessionAsync(_sessionId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-save failed: {ex.Message}");
            }
        }

        public async Task SaveAsync()
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                await _globalConversationService.SaveSessionAsync(_sessionId);
            }
        }

        public Message[] GetApiMessages()
        {
            lock (_lock)
            {
                return _messages.ToArray();
            }
        }

        public void LoadMessages(IEnumerable<Message> messages)
        {
            lock (_lock)
            {
                _messages.Clear();
                _messages.AddRange(messages);
            }
        }

        public IEnumerable<Message> GetHistory()
        {
            lock (_lock)
            {
                return _messages.AsReadOnly();
            }
        }

        public IReadOnlyList<Message> GetFullHistory()
        {
            lock (_lock)
            {
                return _messages.AsReadOnly();
            }
        }

        public void ClearHistory()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
        }
    }
}
