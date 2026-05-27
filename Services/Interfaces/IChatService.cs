using Bloomy.Models;
using Bloomy.DTOs.Chat;

namespace Bloomy.Services.Interfaces
{
    public interface IChatService
    {
        // Conversation methods
        Task<ChatConversationDto?> GetConversationAsync(Guid conversationId, Guid userId);
        Task<List<ChatConversationDto>> GetUserConversationsAsync(Guid userId);
        Task<ChatConversationDetailDto?> GetConversationDetailAsync(Guid conversationId, Guid userId, int page = 1);
        Task<ChatConversationDto> StartConversationAsync(Guid customerId, StartConversationDto dto);

        // Message methods
        Task<ChatMessage> SendMessageAsync(Guid conversationId, Guid senderId, string message, string? imageUrl = null);
        Task MarkMessageAsReadAsync(Guid messageId);
        Task MarkAllConversationMessagesAsReadAsync(Guid conversationId, Guid userId);
        Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);

        // File upload
        Task<string> UploadChatImageAsync(IFormFile file);
    }
}
