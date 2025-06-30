using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.Core.Services
{
    public class ConversationService : IAutoSaveConversation
    {
        private readonly object _lock = new();
        private readonly int _maxHistoryLength;
        private readonly IGlobalConversationService _globalConversationService;
        private readonly List<Message> _messages = new();

        private string _sessionId;

        public const string DefaultModel = "llama-3.3-70b-versatile";
        public string CurrentModel { get; set; } = DefaultModel;

        public ConversationService(
            IGlobalConversationService globalConversationService,
            int maxHistoryLength = 100)
        {
            _globalConversationService = globalConversationService
                ?? throw new ArgumentNullException(nameof(globalConversationService));
            _maxHistoryLength = maxHistoryLength;
        }

        public void LoadFromSession(ConversationSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            _sessionId = session.SessionId;
            CurrentModel = session.Model ?? DefaultModel;

            if (session.Messages?.Any() == true)
            {
                LoadMessages(session.Messages);
            }
        }

        public void AddMessage(string role, string content)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            lock (_lock)
            {
                _messages.Add(new Message { Role = role, Content = content });

                while (_messages.Count > _maxHistoryLength)
                {
                    _messages.RemoveAt(0);
                }
            }

            _ = TryAutoSave();
        }

        public void AddMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            AddMessage(message.Role, message.Content?.ToString() ?? string.Empty);
        }

        public Message[] GetApiMessages()
        {
            lock (_lock)
            {
                return _messages.ToArray();
            }
        }

        public IReadOnlyList<Message> GetFullHistory()
        {
            lock (_lock)
            {
                return _messages.AsReadOnly();
            }
        }

        public IEnumerable<Message> GetHistory() => GetFullHistory();

        public void LoadMessages(IEnumerable<Message> messages)
        {
            if (messages == null) return;

            lock (_lock)
            {
                _messages.Clear();
                _messages.AddRange(messages);
            }
        }

        public void ClearHistory()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
        }

        public async Task SaveAsync()
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                await _globalConversationService.SaveSessionAsync(_sessionId, this);
            }
        }

        private async Task TryAutoSave()
        {
            try
            {
                if (!string.IsNullOrEmpty(_sessionId))
                {
                    await _globalConversationService.SaveSessionAsync(_sessionId, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-save failed: {ex.Message}");
            }
        }
    }
}
