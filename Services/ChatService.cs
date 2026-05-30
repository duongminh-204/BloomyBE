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

            if (!Directory.Exists(_uploadsDirectory))
                Directory.CreateDirectory(_uploadsDirectory);
        }

        public async Task<ChatConversationDto?> GetConversationAsync(Guid conversationId, Guid userId, Guid? shopId = null)
        {
            if (!await _chatRepository.CanAccessConversationAsync(conversationId, userId, shopId))
                return null;

            var conversation = await _chatRepository.GetConversationAsync(conversationId);
            if (conversation == null) return null;

            var unreadCount = await _chatRepository.GetUnreadMessageCountAsync(conversationId, userId);
            return MapConversation(conversation, unreadCount);
        }

        public async Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId, Guid? shopId = null)
        {
            var conversations = shopId.HasValue
                ? await _chatRepository.GetShopConversationsAsync(shopId.Value)
                : await _chatRepository.GetCustomerConversationsAsync(userId);

            var result = new List<ChatConversationDto>();
            foreach (var conversation in conversations)
            {
                var unreadCount = await _chatRepository.GetUnreadMessageCountAsync(conversation.Id, userId);
                result.Add(MapConversation(conversation, unreadCount));
            }

            return result;
        }

        public async Task<ChatConversationDetailDto?> GetConversationDetailAsync(Guid conversationId, Guid userId, int page = 1, Guid? shopId = null)
        {
            if (!await _chatRepository.CanAccessConversationAsync(conversationId, userId, shopId))
                return null;

            var conversation = await _chatRepository.GetConversationAsync(conversationId);
            if (conversation == null) return null;

            await _chatRepository.MarkAllConversationMessagesAsReadAsync(conversationId, userId);
            var messages = await _chatRepository.GetConversationMessagesAsync(conversationId, pageSize: 50, pageNumber: page);

            return new ChatConversationDetailDto
            {
                Id = conversation.Id,
                CustomerId = conversation.CustomerId,
                ShopId = conversation.ShopId,
                ShopName = conversation.Shop?.Name ?? string.Empty,
                CustomerName = conversation.Customer?.FullName ?? "Unknown",
                ShopOwnerName = conversation.Shop?.Owner?.FullName ?? "Unknown",
                CustomerAvatar = conversation.Customer?.Avatar,
                ShopOwnerAvatar = conversation.Shop?.Owner?.Avatar,
                ShopLogoUrl = conversation.Shop?.LogoUrl,
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
            if (dto.ShopId == Guid.Empty)
                throw new InvalidOperationException("Vui lòng chọn shop để chat.");

            var conversation = await _chatRepository.GetOrCreateConversationAsync(
                customerId,
                dto.ShopId,
                dto.OrderId
            );

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

            return await GetConversationAsync(conversation.Id, customerId)
                ?? throw new InvalidOperationException("Failed to get conversation");
        }

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

        public Task MarkMessageAsReadAsync(Guid messageId) =>
            _chatRepository.MarkMessageAsReadAsync(messageId);

        public Task MarkAllConversationMessagesAsReadAsync(Guid conversationId, Guid userId) =>
            _chatRepository.MarkAllConversationMessagesAsReadAsync(conversationId, userId);

        public Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId) =>
            _chatRepository.GetUnreadMessageCountAsync(conversationId, userId);

        public async Task<string> UploadChatImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only images are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit");

            try
            {
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsDirectory, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return $"/uploads/chat/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading chat image");
                throw;
            }
        }

        private static ChatConversationDto MapConversation(ChatConversation conversation, int unreadCount) =>
            new()
            {
                Id = conversation.Id,
                CustomerId = conversation.CustomerId,
                ShopId = conversation.ShopId,
                ShopName = conversation.Shop?.Name ?? string.Empty,
                CustomerName = conversation.Customer?.FullName ?? "Unknown",
                ShopOwnerName = conversation.Shop?.Owner?.FullName ?? "Unknown",
                CustomerAvatar = conversation.Customer?.Avatar,
                ShopOwnerAvatar = conversation.Shop?.Owner?.Avatar,
                ShopLogoUrl = conversation.Shop?.LogoUrl,
                OrderId = conversation.OrderId,
                OrderTitle = conversation.Order?.EventName,
                LastMessageAt = conversation.LastMessageAt,
                UnreadCount = unreadCount,
                IsActive = conversation.IsActive
            };
    }
}
