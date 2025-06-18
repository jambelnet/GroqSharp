using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Services;
using GroqSharp.WebAPI.Models;
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
        public async Task<IActionResult> SetModel([FromBody] SetModelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Model))
                return BadRequest("Model value is required.");

            var availableModels = await _groqService.GetAvailableModelsAsync();
            if (!availableModels.Contains(request.Model))
                return NotFound($"Model '{request.Model}' is not available.");

            _conversation.CurrentModel = request.Model;
            return Ok($"Model set to {request.Model}");
        }
    }
}
