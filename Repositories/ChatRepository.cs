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

        public async Task<ChatConversation?> GetConversationAsync(Guid conversationId)
        {
            return await _context.ChatConversations
                .Include(c => c.Customer)
                .Include(c => c.Shop)
                    .ThenInclude(s => s.Owner)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<ChatConversation?> GetOrCreateConversationAsync(Guid customerId, Guid shopId, Guid? orderId = null)
        {
            var existingConversation = await _context.ChatConversations
                .Where(c => c.CustomerId == customerId && c.ShopId == shopId)
                .FirstOrDefaultAsync();

            if (existingConversation != null)
            {
                existingConversation.IsActive = true;
                await _context.SaveChangesAsync();
                return existingConversation;
            }

            var newConversation = new ChatConversation
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ShopId = shopId,
                OrderId = orderId,
                LastMessageAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ChatConversations.Add(newConversation);
            await _context.SaveChangesAsync();

            return newConversation;
        }

        public async Task<List<ChatConversation>> GetCustomerConversationsAsync(Guid customerId)
        {
            return await _context.ChatConversations
                .Where(c => c.CustomerId == customerId && c.IsActive)
                .Include(c => c.Customer)
                .Include(c => c.Shop)
                    .ThenInclude(s => s.Owner)
                .Include(c => c.Order)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<ChatConversation>> GetShopConversationsAsync(Guid shopId)
        {
            return await _context.ChatConversations
                .Where(c => c.ShopId == shopId && c.IsActive)
                .Include(c => c.Customer)
                .Include(c => c.Shop)
                    .ThenInclude(s => s.Owner)
                .Include(c => c.Order)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<bool> CanAccessConversationAsync(Guid conversationId, Guid userId, Guid? shopId)
        {
            var conversation = await _context.ChatConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return false;
            if (conversation.CustomerId == userId) return true;
            if (shopId.HasValue && conversation.ShopId == shopId.Value) return true;
            return false;
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

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            var conversation = await _context.ChatConversations.FindAsync(message.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

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
                message.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadMessageCountAsync(Guid conversationId, Guid userId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);
        }
    }
}
