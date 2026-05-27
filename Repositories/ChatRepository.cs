using Bloomy.Data.Interfaces;
using Bloomy.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloomy.Data.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly BloomyDbContext _context;

        public ChatRepository(BloomyDbContext context)
        {
            _context = context;
        }

        // ==================== CONVERSATION METHODS ====================

        public async Task<ChatConversation?> GetConversationAsync(Guid conversationId)
        {
            return await _context.ChatConversations
                .Include(c => c.Customer)
                .Include(c => c.ShopOwner)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<ChatConversation?> GetOrCreateConversationAsync(Guid customerId, Guid shopOwnerId, Guid? orderId = null)
        {
            // Tìm conversation hiện có giữa customer và shop owner
            var existingConversation = await _context.ChatConversations
                .Where(c => c.CustomerId == customerId && c.ShopOwnerId == shopOwnerId)
                .FirstOrDefaultAsync();

            if (existingConversation != null)
            {
                existingConversation.IsActive = true;
                await _context.SaveChangesAsync();
                return existingConversation;
            }

            // Tạo conversation mới
            var newConversation = new ChatConversation
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ShopOwnerId = shopOwnerId,
                OrderId = orderId,
                LastMessageAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ChatConversations.Add(newConversation);
            await _context.SaveChangesAsync();

            return newConversation;
        }

        public async Task<List<ChatConversation>> GetUserConversationsAsync(Guid userId)
        {
            return await _context.ChatConversations
                .Where(c => (c.CustomerId == userId || c.ShopOwnerId == userId) && c.IsActive)
                .Include(c => c.Customer)
                .Include(c => c.ShopOwner)
                .Include(c => c.Order)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<ChatConversation> CreateConversationAsync(ChatConversation conversation)
        {
            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task UpdateConversationAsync(ChatConversation conversation)
        {
            _context.ChatConversations.Update(conversation);
            await _context.SaveChangesAsync();
        }

        // ==================== MESSAGE METHODS ====================

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Update LastMessageAt in conversation
            var conversation = await _context.ChatConversations.FindAsync(message.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Load sender info
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            return message;
        }

        public async Task<ChatMessage?> GetMessageAsync(Guid messageId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        public async Task<List<ChatMessage>> GetConversationMessagesAsync(Guid conversationId, int pageSize = 50, int pageNumber = 1)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Reverse()
                .ToListAsync();
        }

        public async Task MarkMessageAsReadAsync(Guid messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllConversationMessagesAsReadAsync(Guid conversationId, Guid userId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadMessageCountAsync(Guid conversationId, Guid userId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);
        }
    }
}
