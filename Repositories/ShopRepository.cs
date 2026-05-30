using Bloomy.Data;
using Bloomy.Models;
using BloomyBE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly BloomyDbContext _db;

        public ShopRepository(BloomyDbContext db) => _db = db;

        public Task<Shop?> GetByIdAsync(Guid shopId, CancellationToken ct = default) =>
            _db.Shops.AsNoTracking()
                .Include(s => s.Owner)
                .FirstOrDefaultAsync(s => s.Id == shopId, ct);

        public Task<Shop?> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default) =>
            _db.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId, ct);

        public Task<List<Shop>> GetAllAsync(CancellationToken ct = default) =>
            _db.Shops.AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);

        public async Task<Shop> CreateAsync(Shop shop, CancellationToken ct = default)
        {
            _db.Shops.Add(shop);
            await _db.SaveChangesAsync(ct);
            return shop;
        }

        public async Task UpdateAsync(Shop shop, CancellationToken ct = default)
        {
            _db.Shops.Update(shop);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> IsOwnedByAsync(Guid shopId, Guid ownerId, CancellationToken ct = default) =>
            _db.Shops.AnyAsync(s => s.Id == shopId && s.OwnerId == ownerId, ct);
    }
}
