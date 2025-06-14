using GroqSharp.Core;
using GroqSharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroqSharp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/model")]
    public class ModelController : ControllerBase
    {
        private readonly IGroqService _groqService;
        private readonly ConversationService _conversation;

        public ModelController(IGroqService groqService, ConversationService conversation)
        {
            _groqService = groqService;
            _conversation = conversation;
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListModels()
        {
            var models = await _groqService.GetAvailableModelsAsync();
            return Ok(models);
        }

        [HttpPost("set")]
        public IActionResult SetModel([FromBody] string model)
        {
            _conversation.CurrentModel = model;
            return Ok($"Model set to {model}");
        }
    }
}
