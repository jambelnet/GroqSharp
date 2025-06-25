using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using GroqSharp.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroqSharp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/conversations/{sessionId}/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IGlobalConversationService _conversationService;
        private readonly IGroqService _groqService;
        private readonly IConfiguration _config;

        public ChatController(
            IGlobalConversationService conversationService,
            IGroqService groqService,
            IConfiguration config)
        {
            _conversationService = conversationService;
            _groqService = groqService;
            _config = config;
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(string sessionId, [FromBody] Message userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage?.Content))
                return BadRequest("Message content cannot be empty.");

            var sessionContext = await SessionContext.CreateAsync(sessionId, _conversationService);

            sessionContext.Conversation.AddMessage("user", userMessage.Content);

            var response = await _groqService.GetChatCompletionAsync(new ChatRequest
            {
                Model = sessionContext.Conversation.CurrentModel,
                Messages = sessionContext.Conversation.GetApiMessages(),
                Temperature = double.TryParse(_config["Groq:DefaultTemperature"], out var temp) ? temp : 0.7
            });

            sessionContext.Conversation.AddMessage("assistant", response);
            await _conversationService.SaveSessionAsync(sessionId);

            return Ok(new { response });
        }
    }
}
