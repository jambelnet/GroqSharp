using GroqSharp.Services;
using GroqSharp.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GroqSharp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/archives")]
    public class ArchiveController : ControllerBase
    {
        private readonly ConversationPersistenceService _persistence;

        public ArchiveController(ConversationPersistenceService persistence)
        {
            _persistence = persistence;
        }

        [HttpGet]
        public IActionResult List()
        {
            var archives = _persistence.ListArchives();
            return Ok(archives);
        }

        [HttpPost("load")]
        public IActionResult Load([FromBody] ArchiveDto dto)
        {
            if (_persistence.TryLoadArchive(dto.Id, out var conversation, out _))
                return Ok(conversation.GetHistory());

            return NotFound("Archive not found");
        }

        [HttpPost("rename")]
        public IActionResult Rename([FromBody] ArchiveDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("New name is required");

            if (_persistence.RenameArchive(dto.Id, dto.Title))
                return Ok("Archive renamed");

            return BadRequest("Rename failed");
        }

        [HttpDelete]
        public IActionResult Delete([FromBody] ArchiveDto dto)
        {
            if (_persistence.DeleteArchive(dto.Id))
                return Ok("Archive deleted");

            return NotFound("Archive not found");
        }
    }
}
