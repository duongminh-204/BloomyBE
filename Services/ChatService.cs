using Bloomy.Data.Interfaces;
using Bloomy.Models;
using Bloomy.Services.Interfaces;
using Bloomy.DTOs.Chat;

namespace Bloomy.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatService> _logger;
        private readonly string _uploadsDirectory;

        public ChatService(IChatRepository chatRepository, ILogger<ChatService> logger, IWebHostEnvironment environment)
        {
            _chatRepository = chatRepository;
            _logger = logger;
            _uploadsDirectory = Path.Combine(environment.WebRootPath, "uploads", "chat");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(_uploadsDirectory))
            {
                Directory.CreateDirectory(_uploadsDirectory);
            }
        }

        // ==================== CONVERSATION METHODS ====================

        public async Task<ChatConversationDto?> GetConversationAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _chatRepository.GetConversationAsync(conversationId);

            if (conversation == null)
                return null;

            // Kiểm tra user có quyền truy cập conversation này không
            if (conversation.CustomerId != userId && conversation.ShopOwnerId != userId)
                return null;

            var unreadCount = await _chatRepository.GetUnreadMessageCountAsync(conversationId, userId);

            return new ChatConversationDto
            {
                Id = conversation.Id,
                CustomerId = conversation.CustomerId,
                ShopOwnerId = conversation.ShopOwnerId,
                CustomerName = conversation.Customer?.FullName ?? "Unknown",
                ShopOwnerName = conversation.ShopOwner?.FullName ?? "Unknown",
                CustomerAvatar = conversation.Customer?.Avatar,
                ShopOwnerAvatar = conversation.ShopOwner?.Avatar,
                OrderId = conversation.OrderId,
                OrderTitle = conversation.Order?.EventName,
                LastMessageAt = conversation.LastMessageAt,
                UnreadCount = unreadCount,
                IsActive = conversation.IsActive
            };
        }

        public async Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId)
        {
            var conversations = await _chatRepository.GetUserConversationsAsync(userId);
            var result = new List<ChatConversationDto>();

            foreach (var conversation in conversations)
            {
                var unreadCount = await _chatRepository.GetUnreadMessageCountAsync(conversation.Id, userId);
                
                result.Add(new ChatConversationDto
                {
                    Id = conversation.Id,
                    CustomerId = conversation.CustomerId,
                    ShopOwnerId = conversation.ShopOwnerId,
                    CustomerName = conversation.Customer?.FullName ?? "Unknown",
                    ShopOwnerName = conversation.ShopOwner?.FullName ?? "Unknown",
                    CustomerAvatar = conversation.Customer?.Avatar,
                    ShopOwnerAvatar = conversation.ShopOwner?.Avatar,
                    OrderId = conversation.OrderId,
                    OrderTitle = conversation.Order?.EventName,
                    LastMessageAt = conversation.LastMessageAt,
                    UnreadCount = unreadCount,
                    IsActive = conversation.IsActive
                });
            }

            return result;
        }

        public async Task<ChatConversationDetailDto?> GetConversationDetailAsync(Guid conversationId, Guid userId, int page = 1)
        {
            var conversation = await _chatRepository.GetConversationAsync(conversationId);

            if (conversation == null)
                return null;

            // Kiểm tra user có quyền truy cập conversation này không
            if (conversation.CustomerId != userId && conversation.ShopOwnerId != userId)
                return null;

            // Đánh dấu tất cả tin nhắn là đã đọc
            await _chatRepository.MarkAllConversationMessagesAsReadAsync(conversationId, userId);

            var messages = await _chatRepository.GetConversationMessagesAsync(conversationId, pageSize: 50, pageNumber: page);

            return new ChatConversationDetailDto
            {
                Id = conversation.Id,
                CustomerId = conversation.CustomerId,
                ShopOwnerId = conversation.ShopOwnerId,
                CustomerName = conversation.Customer?.FullName ?? "Unknown",
                ShopOwnerName = conversation.ShopOwner?.FullName ?? "Unknown",
                CustomerAvatar = conversation.Customer?.Avatar,
                ShopOwnerAvatar = conversation.ShopOwner?.Avatar,
                OrderId = conversation.OrderId,
                OrderTitle = conversation.Order?.EventName,
                Messages = messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.FullName ?? "Unknown",
                    SenderAvatar = m.Sender?.Avatar ?? string.Empty,
                    Message = m.Message,
                    ImageUrl = m.ImageUrl,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt
                }).ToList()
            };
        }

        public async Task<ChatConversationDto> StartConversationAsync(Guid customerId, StartConversationDto dto)
        {
            var conversation = await _chatRepository.GetOrCreateConversationAsync(
                customerId,
                dto.ShopOwnerId,
                dto.OrderId
            );

            // Gửi tin nhắn đầu tiên nếu có
            if (!string.IsNullOrEmpty(dto.InitialMessage))
            {
                await _chatRepository.AddMessageAsync(new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    SenderId = customerId,
                    Message = dto.InitialMessage,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                });
            }

            return await GetConversationAsync(conversation.Id, customerId) ?? 
                   throw new Exception("Failed to get conversation");
        }

        // ==================== MESSAGE METHODS ====================

        public async Task<ChatMessage> SendMessageAsync(Guid conversationId, Guid senderId, string message, string? imageUrl = null)
        {
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = senderId,
                Message = message,
                ImageUrl = imageUrl,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            return await _chatRepository.AddMessageAsync(chatMessage);
        }

        public async Task MarkMessageAsReadAsync(Guid messageId)
        {
            await _chatRepository.MarkMessageAsReadAsync(messageId);
        }

        public async Task MarkAllConversationMessagesAsReadAsync(Guid conversationId, Guid userId)
        {
            await _chatRepository.MarkAllConversationMessagesAsReadAsync(conversationId, userId);
        }

        public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
        {
            return await _chatRepository.GetUnreadMessageCountAsync(conversationId, userId);
        }

        // ==================== FILE UPLOAD ====================

        public async Task<string> UploadChatImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only images are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit");

            try
            {
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/chat/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading chat image");
                throw;
            }
        }
    }
}
