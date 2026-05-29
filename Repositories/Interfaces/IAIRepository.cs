using Bloomy.Models;
using Bloomy.Models.Enums;

namespace BloomyBE.Repositories.Interfaces
{
    public interface IAIRepository
    {
        Task<AIConversation?> GetConversationAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<AIConversation?> GetConversationWithMessagesAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<List<AIConversation>> GetUserConversationsAsync(Guid userId, int limit = 20, CancellationToken ct = default);
        Task<AIConversation> CreateConversationAsync(AIConversation conversation, CancellationToken ct = default);
        Task UpdateConversationAsync(AIConversation conversation, CancellationToken ct = default);
        Task<AIMessage> AddMessageAsync(AIMessage message, CancellationToken ct = default);
        Task UpdateMessageAsync(AIMessage message, CancellationToken ct = default);
        Task<SavedConcept> SaveConceptAsync(SavedConcept concept, CancellationToken ct = default);
        Task<List<SavedConcept>> GetSavedConceptsAsync(Guid userId, CancellationToken ct = default);
        Task<SavedConcept?> GetSavedConceptAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task DeleteSavedConceptAsync(Guid id, CancellationToken ct = default);
        Task<int> GetDailyUsageCountAsync(Guid userId, AIUsageType type, DateTime usageDate, CancellationToken ct = default);
        Task RecordUsageAsync(AIUsage usage, CancellationToken ct = default);
        Task<List<PortfolioItem>> GetPortfolioItemsForMatchingAsync(CancellationToken ct = default);
    }
}
