using Bloomy.Data;
using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Repositories
{
    public class AIRepository : IAIRepository
    {
        private readonly BloomyDbContext _db;

        public AIRepository(BloomyDbContext db) => _db = db;

        public Task<AIConversation?> GetConversationAsync(Guid id, Guid userId, CancellationToken ct = default) =>
            _db.AIConversations.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        public Task<AIConversation?> GetConversationWithMessagesAsync(Guid id, Guid userId, CancellationToken ct = default) =>
            _db.AIConversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        public Task<List<AIConversation>> GetUserConversationsAsync(Guid userId, int limit = 20, CancellationToken ct = default) =>
            _db.AIConversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(limit)
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .ToListAsync(ct);

        public async Task<AIConversation> CreateConversationAsync(AIConversation conversation, CancellationToken ct = default)
        {
            _db.AIConversations.Add(conversation);
            await _db.SaveChangesAsync(ct);
            return conversation;
        }

        public async Task UpdateConversationAsync(AIConversation conversation, CancellationToken ct = default)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
            _db.AIConversations.Update(conversation);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<AIMessage> AddMessageAsync(AIMessage message, CancellationToken ct = default)
        {
            _db.AIMessages.Add(message);
            await _db.SaveChangesAsync(ct);
            return message;
        }

        public async Task UpdateMessageAsync(AIMessage message, CancellationToken ct = default)
        {
            _db.AIMessages.Update(message);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<SavedConcept> SaveConceptAsync(SavedConcept concept, CancellationToken ct = default)
        {
            _db.SavedConcepts.Add(concept);
            await _db.SaveChangesAsync(ct);
            return concept;
        }

        public Task<List<SavedConcept>> GetSavedConceptsAsync(Guid userId, CancellationToken ct = default) =>
            _db.SavedConcepts
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

        public Task<SavedConcept?> GetSavedConceptAsync(Guid id, Guid userId, CancellationToken ct = default) =>
            _db.SavedConcepts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        public async Task DeleteSavedConceptAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.SavedConcepts.FindAsync(new object[] { id }, ct);
            if (entity != null)
            {
                _db.SavedConcepts.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
        }

        public Task<int> GetDailyUsageCountAsync(Guid userId, AIUsageType type, DateTime usageDate, CancellationToken ct = default) =>
            _db.AIUsages
                .Where(u => u.UserId == userId && u.UsageType == type && u.UsageDate == usageDate.Date)
                .SumAsync(u => u.RequestCount, ct);

        public async Task RecordUsageAsync(AIUsage usage, CancellationToken ct = default)
        {
            _db.AIUsages.Add(usage);
            await _db.SaveChangesAsync(ct);
        }

        public Task<List<PortfolioItem>> GetPortfolioItemsForMatchingAsync(CancellationToken ct = default) =>
            _db.PortfolioItems
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.EventType)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
    }
}
