using GroqSharp.Core;
using GroqSharp.Models;
using GroqSharp.Services;
using GroqSharp.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GroqSharp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IGroqService _groqService;
        private readonly ConversationService _conversation;

        public ChatController(IGroqService groqService, ConversationService conversation)
        {
            _groqService = groqService;
            _conversation = conversation;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            try
            {
                _conversation.AddMessage("user", request.Message);

                var response = await _groqService.GetChatCompletionAsync(new ChatRequest
                {
                    Messages = _conversation.GetApiMessages(),
                    Model = _conversation.CurrentModel,
                    Stream = request.Stream
                });

                _conversation.AddMessage("assistant", response);
                return Ok(new { Content = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            try
            {
                //var sessionId = HttpContext.Session.Id;

                var history = _conversation.GetHistory().Select(m => new {
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = DateTime.UtcNow // Optional: add timestamp if needed
                });

                return Ok(new
                {
                    Model = _conversation.CurrentModel,
                    Messages = history
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("reset")]
        public IActionResult Reset()
        {
            try
            {
                _conversation.LoadMessages([]);
                return Ok(new { Message = "Conversation reset." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
