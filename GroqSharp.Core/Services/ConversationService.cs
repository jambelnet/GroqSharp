using GroqSharp.Core.Enums;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.Core.Services
{
    /// <summary>
    /// Manages conversation messages with auto-saving and history limits.
    /// </summary>
    public class ConversationService : IAutoSaveConversation
    {
        private readonly IGlobalConversationService _globalConversationService;
        private readonly IModelResolver _modelResolver;
        private readonly List<Message> _messages = new();
        private readonly object _lock = new();
        private readonly int _maxHistoryLength;      
        private string? _sessionId;

        /// <summary>Current model in use for the conversation.</summary>
        public string CurrentModel { get; set; }

        public ConversationService(
            IGlobalConversationService globalConversationService,
            IModelResolver modelResolver,
            int maxHistoryLength = 100)
        {
            _globalConversationService = globalConversationService
                ?? throw new ArgumentNullException(nameof(globalConversationService));
            _modelResolver = modelResolver
                ?? throw new ArgumentNullException(nameof(modelResolver));
            _maxHistoryLength = maxHistoryLength;

            // Use config-driven model on creation
            CurrentModel = _modelResolver.GetModelFor(GroqFeature.Default);
        }

        /// <summary>
        /// Loads conversation data from a saved session.
        /// </summary>
        public void LoadFromSession(ConversationSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            _sessionId = session.SessionId;
            CurrentModel = session.Model ?? _modelResolver.GetModelFor(GroqFeature.Default);

            if (session.Messages?.Any() == true)
            {
                LoadMessages(session.Messages);
            }
        }

        /// <summary>
        /// Internal shared method to add a message safely and trim history.
        /// </summary>
        private void AddMessageInternal(Message message)
        {
            lock (_lock)
            {
                _messages.Add(message);

                while (_messages.Count > _maxHistoryLength)
                {
                    _messages.RemoveAt(0);
                }
            }

            _ = TryAutoSave();
        }

        /// <summary>
        /// Adds a message by role and content.
        /// </summary>
        public void AddMessage(MessageRole role, string content)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(role.ToString());
            AddMessageInternal(new Message { Role = role, Content = content });
        }

        /// <summary>
        /// Adds a message object.
        /// </summary>
        public void AddMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            AddMessageInternal(message);
        }

        /// <summary>
        /// Gets a snapshot of messages suitable for API calls.
        /// </summary>
        public Message[] GetApiMessages()
        {
            lock (_lock)
            {
                return _messages.ToArray();
            }
        }

        /// <summary>
        /// Gets a read-only list of all messages in history.
        /// </summary>
        public IReadOnlyList<Message> GetFullHistory()
        {
            lock (_lock)
            {
                return _messages.AsReadOnly();
            }
        }

        /// <summary>
        /// Loads a new set of messages, replacing existing history.
        /// </summary>
        public void LoadMessages(IEnumerable<Message> messages)
        {
            if (messages?.Any() != true)
                return;

            lock (_lock)
            {
                _messages.Clear();
                _messages.AddRange(messages);
            }
        }

        /// <summary>
        /// Clears the conversation history.
        /// </summary>
        public void ClearHistory()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
        }

        /// <summary>
        /// Saves the current conversation asynchronously.
        /// </summary>
        public async Task SaveAsync()
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                await _globalConversationService.SaveSessionAsync(_sessionId, this);
            }
        }

        /// <summary>
        /// Attempts an auto-save, logging any failures.
        /// </summary>
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
