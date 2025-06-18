using GroqSharp.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GroqSharp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : ControllerBase
    {
        private readonly IGlobalConversationService _conversationService;

        public ConversationController(IGlobalConversationService conversationService)
        {
            _conversationService = conversationService;
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
            session.Title = title ?? session.Title;
            await _conversationService.SaveSessionAsync(sessionId);
            return Ok(new { message = "New session started.", sessionId, title = session.Title });
        }

        [HttpPost("{sessionId}/clear")]
        public async Task<IActionResult> Clear(string sessionId)
        {
            var session = await _conversationService.GetOrCreateSessionAsync(sessionId);
            session.Conversation.ClearHistory();
            await _conversationService.SaveSessionAsync(sessionId);
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
            return Ok(new { session.Title, session.Conversation });
        }

        [HttpGet("{sessionId}/history")]
        public async Task<IActionResult> GetHistory(string sessionId)
        {
            var session = await _conversationService.GetOrCreateSessionAsync(sessionId);
            var history = session.Conversation.GetFullHistory();

            return Ok(history);
        }
    }
}
