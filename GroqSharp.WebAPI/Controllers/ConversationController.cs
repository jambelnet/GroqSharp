using GroqSharp.Core.Interfaces;
using GroqSharp.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroqSharp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : ControllerBase
    {
        private readonly IGlobalConversationService _conversationService;
        private readonly IModelResolver _modelResolver;

        public ConversationController(IGlobalConversationService conversationService, IModelResolver modelResolver)
        {
            _conversationService = conversationService;
            _modelResolver = modelResolver;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var list = await _conversationService.ListAllConversationsAsync();
            return Ok(list);
        }

        [HttpPost("{sessionId}/new")]
        public async Task<IActionResult> NewSession(string sessionId, [FromQuery] string? title)
        {
            var session = await _conversationService.GetOrCreateSessionAsync(sessionId);
            var sessionContext = await SessionContext.CreateAsync(sessionId, _conversationService, _modelResolver);
            session.Title = title ?? session.Title;
            await _conversationService.SaveSessionAsync(sessionId, sessionContext.Conversation);
            return Ok(new { message = "New session started.", sessionId, title = session.Title });
        }

        [HttpPost("{sessionId}/clear")]
        public async Task<IActionResult> Clear(string sessionId)
        {
            var sessionContext = await SessionContext.CreateAsync(sessionId, _conversationService, _modelResolver);
            sessionContext.Conversation.ClearHistory();
            await _conversationService.SaveSessionAsync(sessionId, sessionContext.Conversation);
            return Ok(new { message = "Conversation cleared." });
        }

        [HttpPost("{sessionId}/rename")]
        public async Task<IActionResult> Rename(string sessionId, [FromQuery] string newTitle)
        {
            await _conversationService.RenameConversationAsync(sessionId, newTitle);
            return Ok(new { message = "Session renamed.", newTitle });
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> Delete(string sessionId)
        {
            await _conversationService.DeleteConversationAsync(sessionId);
            return Ok(new { message = "Session deleted.", sessionId });
        }

        [HttpGet("{sessionId}/load")]
        public async Task<IActionResult> Load(string sessionId)
        {
            var session = await _conversationService.GetOrCreateSessionAsync(sessionId);
            var sessionContext = await SessionContext.CreateAsync(sessionId, _conversationService, _modelResolver);
            return Ok(new { session.Title, sessionContext.Conversation });
        }

        [HttpGet("{sessionId}/history")]
        public async Task<IActionResult> GetHistory(string sessionId)
        {
            var sessionContext = await SessionContext.CreateAsync(sessionId, _conversationService, _modelResolver);
            var history = sessionContext.Conversation.GetFullHistory();
            return Ok(history);
        }
    }
}
