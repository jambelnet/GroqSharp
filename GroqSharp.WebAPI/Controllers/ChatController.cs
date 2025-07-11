using GroqSharp.Core.Builders;
using GroqSharp.Core.Configuration.Interfaces;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
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
        private readonly IGroqConfigurationService _configurationService;
        private readonly IGroqService _groqService;
        private readonly IConfiguration _config;
        private readonly IModelResolver _modelResolver;

        public ChatController(
            IGlobalConversationService conversationService,
            IGroqConfigurationService configurationService,
            IGroqService groqService,
            IConfiguration config,
            IModelResolver modelResolver)
        {
            _conversationService = conversationService;
            _configurationService = configurationService;
            _groqService = groqService;
            _config = config;
            _modelResolver = modelResolver;
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(string sessionId, [FromBody] Message userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage?.Content))
                return BadRequest("Message content cannot be empty.");

            var sessionContext = await SessionContext.CreateAsync(sessionId, _conversationService, _modelResolver);

            sessionContext.Conversation.AddMessage(MessageRole.User, userMessage.Content);

            var defaults = _configurationService.GetDefaultsFor(GroqFeature.Default);
            var temperature = defaults.Temperature;
            var maxTokens = defaults.MaxTokens;

            var request = new ChatRequestBuilder()
                .WithModel(sessionContext.Conversation.CurrentModel)
                .WithMessages(sessionContext.Conversation.GetApiMessages().SanitizeForApi())
                .WithTemperature(temperature)
                .WithMaxTokens(maxTokens)
                .Build();

            var response = await _groqService.GetChatCompletionAsync(request);

            sessionContext.Conversation.AddMessage(MessageRole.Assistant, response);
            await _conversationService.SaveSessionAsync(sessionId, sessionContext.Conversation);

            return Ok(new { response });
        }
    }
}
