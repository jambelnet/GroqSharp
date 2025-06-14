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

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            _conversation.AddMessage("user", request.Message);

            var response = await _groqService.GetChatCompletionAsync(new ChatRequest
            {
                Messages = _conversation.GetApiMessages(),
                Model = _conversation.CurrentModel,
                Stream = request.Stream
            });

            _conversation.AddMessage("assistant", response);
            return Ok(response);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_conversation.GetHistory());
        }

        [HttpPost("reset")]
        public IActionResult Reset()
        {
            _conversation.LoadMessages([]);
            return Ok("Conversation reset.");
        }
    }
}
