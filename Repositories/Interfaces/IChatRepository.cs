using Bloomy.Models;

namespace Bloomy.Data.Interfaces
{
    public interface IChatRepository
    {
        // Conversation methods
        Task<ChatConversation?> GetConversationAsync(Guid conversationId);
        Task<ChatConversation?> GetOrCreateConversationAsync(Guid customerId, Guid shopOwnerId, Guid? orderId = null);
        Task<List<ChatConversation>> GetUserConversationsAsync(Guid userId);
        Task<ChatConversation> CreateConversationAsync(ChatConversation conversation);
        Task UpdateConversationAsync(ChatConversation conversation);

        // Message methods
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageAsync(Guid messageId);
        Task<List<ChatMessage>> GetConversationMessagesAsync(Guid conversationId, int pageSize = 50, int pageNumber = 1);
        Task MarkMessageAsReadAsync(Guid messageId);
        Task MarkAllConversationMessagesAsReadAsync(Guid conversationId, Guid userId);
        Task<int> GetUnreadMessageCountAsync(Guid conversationId, Guid userId);
    }
}
