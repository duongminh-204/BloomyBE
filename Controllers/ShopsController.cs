using BloomyBE.DTOs.Shop;
using BloomyBE.Repositories.Interfaces;
using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/shops")]
    public class ShopsController : ControllerBase
    {
        private readonly IShopRepository _shopRepo;
        private readonly ICurrentShopContext _shopContext;

        public ShopsController(IShopRepository shopRepo, ICurrentShopContext shopContext)
        {
            _shopRepo = shopRepo;
            _shopContext = shopContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var shops = await _shopRepo.GetAllAsync(ct);
            return Ok(shops.Select(s => new ShopListItemDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                Address = s.Address,
                PhoneNumber = s.PhoneNumber,
                CreatedAt = s.CreatedAt
            }));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var shop = await _shopRepo.GetByIdAsync(id, ct);
            if (shop == null)
                return NotFound(new { message = "Không tìm thấy shop." });

            return Ok(new ShopDetailDto
            {
                Id = shop.Id,
                Name = shop.Name,
                Description = shop.Description,
                LogoUrl = shop.LogoUrl,
                Address = shop.Address,
                PhoneNumber = shop.PhoneNumber,
                CreatedAt = shop.CreatedAt,
                OwnerName = shop.Owner?.FullName ?? string.Empty
            });
        }

        [HttpGet("me")]
        [Authorize(Roles = "ShopOwner")]
        public async Task<IActionResult> GetMyShop(CancellationToken ct)
        {
            var shopId = _shopContext.RequireShopId();
            var shop = await _shopRepo.GetByIdAsync(shopId, ct);
            if (shop == null)
                return NotFound(new { message = "Shop không tồn tại." });

            return Ok(new ShopDetailDto
            {
                Id = shop.Id,
                Name = shop.Name,
                Description = shop.Description,
                LogoUrl = shop.LogoUrl,
                Address = shop.Address,
                PhoneNumber = shop.PhoneNumber,
                CreatedAt = shop.CreatedAt,
                OwnerName = shop.Owner?.FullName ?? string.Empty
            });
        }

        [HttpPut("me")]
        [Authorize(Roles = "ShopOwner")]
        public async Task<IActionResult> UpdateMyShop([FromBody] UpdateShopDto dto, CancellationToken ct)
        {
            var shopId = _shopContext.RequireShopId();
            var shop = await _shopRepo.GetByIdAsync(shopId, ct);
            if (shop == null)
                return NotFound(new { message = "Shop không tồn tại." });

            shop.Name = dto.Name.Trim();
            shop.Description = dto.Description?.Trim() ?? shop.Description;
            shop.LogoUrl = dto.LogoUrl?.Trim() ?? shop.LogoUrl;
            shop.Address = dto.Address?.Trim() ?? shop.Address;
            shop.PhoneNumber = dto.PhoneNumber?.Trim() ?? shop.PhoneNumber;

            await _shopRepo.UpdateAsync(shop, ct);
            return Ok(new { message = "Cập nhật shop thành công." });
        }
    }
}
