using Bloomy.Models;

namespace BloomyBE.Repositories.Interfaces
{
    public interface IShopRepository
    {
        Task<Shop?> GetByIdAsync(Guid shopId, CancellationToken ct = default);
        Task<Shop?> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
        Task<List<Shop>> GetAllAsync(CancellationToken ct = default);
        Task<Shop> CreateAsync(Shop shop, CancellationToken ct = default);
        Task UpdateAsync(Shop shop, CancellationToken ct = default);
        Task<bool> IsOwnedByAsync(Guid shopId, Guid ownerId, CancellationToken ct = default);
    }
}
