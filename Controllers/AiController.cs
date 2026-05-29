using BloomyBE.DTOs.AI;
using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize(Roles = "Customer")]
    [EnableRateLimiting("ai")]
    public class AiController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IAIQuotaService _quotaService;

        public AiController(IAIService aiService, IAIQuotaService quotaService)
        {
            _aiService = aiService;
            _quotaService = quotaService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AIChatRequestDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Tin nhắn không được để trống." });

            try
            {
                var result = await _aiService.ChatAsync(GetUserId(), dto, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("analyze-image")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> AnalyzeImage(
            IFormFile image,
            [FromForm] Guid? conversationId,
            CancellationToken ct)
        {
            try
            {
                var result = await _aiService.AnalyzeImageAsync(GetUserId(), image, conversationId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("generate-concept")]
        public async Task<IActionResult> GenerateConcept([FromBody] AIGenerateConceptRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _aiService.GenerateConceptAsync(GetUserId(), dto, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] Guid? conversationId, CancellationToken ct)
        {
            var history = await _aiService.GetHistoryAsync(GetUserId(), conversationId, ct);
            return Ok(history);
        }

        [HttpPost("save-concept")]
        public async Task<IActionResult> SaveConcept([FromBody] SaveAIConceptRequestDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) && dto.ConceptData == null)
                return BadRequest(new { message = "Thiếu thông tin concept." });

            var result = await _aiService.SaveConceptAsync(GetUserId(), dto, ct);
            return Ok(result);
        }

        [HttpGet("saved-concepts")]
        public async Task<IActionResult> GetSavedConcepts(CancellationToken ct)
        {
            var list = await _aiService.GetSavedConceptsAsync(GetUserId(), ct);
            return Ok(list);
        }

        [HttpDelete("saved-concepts/{id:guid}")]
        public async Task<IActionResult> DeleteSavedConcept(Guid id, CancellationToken ct)
        {
            try
            {
                await _aiService.DeleteSavedConceptAsync(GetUserId(), id, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("quota")]
        public async Task<IActionResult> GetQuota(CancellationToken ct)
        {
            var status = await _quotaService.GetQuotaStatusAsync(GetUserId(), ct);
            return Ok(status);
        }
    }
}
