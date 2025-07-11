using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using GroqSharp.Core.Services;

namespace GroqSharp.CLI.Commands.Models
{
    public class CliSessionContext
    {
        private readonly IGlobalConversationService _globalService;
        private readonly IGroqService _groqService;
        private readonly IModelResolver _modelResolver;

        public string? SessionId { get; private set; }
        public string? Title { get; private set; }
        public string? CurrentModel { get; set; }

        public IGroqService GroqService => _groqService;
        public ConversationService? Conversation { get; private set; }
        public bool ShouldExit { get; set; }
        public object? PreviousCommandResult { get; set; }

        public CliSessionContext(
            IGlobalConversationService globalService,
            IGroqService groqService,
            IModelResolver modelResolver)
        {
            _globalService = globalService;
            _groqService = groqService;
            _modelResolver = modelResolver;
        }

        public async Task InitializeAsync(string sessionId, string? title = null)
        {
            SessionId = sessionId;

            ConversationSession session = await _globalService.GetOrCreateSessionAsync(sessionId);

            // Use provided title if new, otherwise fallback to loaded
            Title = title ?? session.Title;
            CurrentModel = session.Model ?? _modelResolver.GetModelFor(GroqFeature.Default);

            // If title was overridden, set it (so it's saved next time)
            session.Title = title ?? session.Title;

            Conversation = new ConversationService(_globalService, _modelResolver);
            Conversation.LoadFromSession(session);
        }

        public Message[] GetSanitizedMessages()
        {
            return Conversation.GetApiMessages().SanitizeForApi().ToArray();
        }

        public string Prompt(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public bool PromptYesNo(string message)
        {
            Console.Write($"{message} (y/n): ");
            var key = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();
            return char.ToLowerInvariant(key) == 'y';
        }

        public async Task SaveConversation()
        {
            await Conversation.SaveAsync();
        }
    }
}
