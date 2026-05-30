using BloomyBE.Configuration;
using System.Security.Claims;

namespace BloomyBE.Services.Interfaces
{
    public interface ICurrentShopContext
    {
        Guid UserId { get; }
        Guid? ShopId { get; }
        bool IsShopOwner { get; }
        Guid RequireShopId();
    }

    public class CurrentShopContext : ICurrentShopContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentShopContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(id))
                    throw new UnauthorizedAccessException("Chưa đăng nhập.");
                return Guid.Parse(id);
            }
        }

        public Guid? ShopId
        {
            get
            {
                var shopId = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(TenantClaimTypes.ShopId);
                return Guid.TryParse(shopId, out var id) ? id : null;
            }
        }

        public bool IsShopOwner =>
            _httpContextAccessor.HttpContext?.User
                .IsInRole("ShopOwner") == true;

        public Guid RequireShopId()
        {
            if (ShopId == null)
                throw new InvalidOperationException("Không tìm thấy Shop trong token. Vui lòng đăng nhập lại.");
            return ShopId.Value;
        }
    }
}
