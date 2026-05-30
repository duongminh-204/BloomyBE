using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bloomy.Services.Interfaces;
using Bloomy.DTOs.Chat;
using BloomyBE.Services.Interfaces;
using System.Security.Claims;

namespace Bloomy.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICurrentShopContext _shopContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ICurrentShopContext shopContext, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _shopContext = shopContext;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Lấy danh sách cuộc trò chuyện của người dùng
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<List<ChatConversationDto>>> GetConversations()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            try
            {
                var shopId = User.IsInRole("ShopOwner") ? _shopContext.ShopId : null;
                var conversations = await _chatService.GetUserConversationsAsync(userId, shopId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new { message = "Error retrieving conversations" });
            }
        }

        /// <summary>
        /// Lấy chi tiết một cuộc trò chuyện cùng các tin nhắn
        /// </summary>
        [HttpGet("conversations/{conversationId}")]
        public async Task<ActionResult<ChatConversationDetailDto>> GetConversation(
            [FromRoute] Guid conversationId,
            [FromQuery] int page = 1)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            try
            {
                var shopId = User.IsInRole("ShopOwner") ? _shopContext.ShopId : null;
                var conversation = await _chatService.GetConversationDetailAsync(conversationId, userId, page, shopId);
                if (conversation == null)
                    return NotFound();

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation detail");
                return StatusCode(500, new { message = "Error retrieving conversation" });
            }
        }

        /// <summary>
        /// Bắt đầu một cuộc trò chuyện mới
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<ChatConversationDto>> StartConversation([FromBody] StartConversationDto dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            if (dto == null || dto.ShopId == Guid.Empty)
                return BadRequest(new { message = "Vui lòng chọn shop để chat." });

            try
            {
                var conversation = await _chatService.StartConversationAsync(userId, dto);
                return Created($"/api/chat/conversations/{conversation.Id}", conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting conversation");
                return StatusCode(500, new { message = "Error creating conversation" });
            }
        }

        /// <summary>
        /// Upload ảnh cho chat
        /// </summary>
        [HttpPost("upload-image")]
        public async Task<ActionResult<object>> UploadChatImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided" });

            try
            {
                var imageUrl = await _chatService.UploadChatImageAsync(file);
                return Ok(new { imageUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = "Error uploading image" });
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn là đã đọc
        /// </summary>
        [HttpPut("messages/{messageId}/mark-as-read")]
        public async Task<ActionResult> MarkMessageAsRead([FromRoute] Guid messageId)
        {
            try
            {
                await _chatService.MarkMessageAsReadAsync(messageId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
                return StatusCode(500, new { message = "Error marking message as read" });
            }
        }

        /// <summary>
        /// Lấy số lượng tin nhắn chưa đọc của một cuộc trò chuyện
        /// </summary>
        [HttpGet("conversations/{conversationId}/unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount([FromRoute] Guid conversationId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            try
            {
                var count = await _chatService.GetUnreadCountAsync(conversationId, userId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { message = "Error retrieving unread count" });
            }
        }
    }
}
