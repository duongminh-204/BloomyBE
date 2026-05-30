using Bloomy.Data.Interfaces;
using Bloomy.DTOs.Auth;
using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.Repositories.Interfaces;
using BloomyBE.Services.Interfaces;
using BloomyBE.Services;

namespace Bloomy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IShopRepository _shopRepo;

        public AuthService(IAuthRepository authRepo, IJwtTokenService jwtTokenService, IShopRepository shopRepo)
        {
            _authRepo = authRepo;
            _jwtTokenService = jwtTokenService;
            _shopRepo = shopRepo;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _authRepo.IsEmailExistAsync(dto.Email))
                throw new InvalidOperationException("Email đã tồn tại.");

            if (await _authRepo.IsPhoneExistAsync(dto.PhoneNumber))
                throw new InvalidOperationException("Số điện thoại đã tồn tại.");

            var user = new User
            {
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                // Đăng ký công khai luôn là Khách hàng; ShopOwner do admin/seed tạo
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _authRepo.CreateAsync(user, dto.Password);

            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetByEmailAsync(dto.EmailOrPhone);

            if (user == null)
                user = await _authRepo.GetByPhoneAsync(dto.EmailOrPhone);

            if (user == null || !await _authRepo.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Email/Số điện thoại hoặc mật khẩu không chính xác.");

            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _authRepo.GetByIdAsync(userId);
            return user == null ? null : await CreateAuthResponse(user);
        }

        private async Task<AuthResponseDto> CreateAuthResponse(User user)
        {
            var role = user.Role switch
            {
                UserRole.ShopOwner => "ShopOwner",
                UserRole.Admin => "Admin",
                _ => "Customer"
            };

            Guid? shopId = null;
            string? shopName = null;
            if (user.Role == UserRole.ShopOwner)
            {
                var shop = await _shopRepo.GetByOwnerIdAsync(user.Id);
                shopId = shop?.Id;
                shopName = shop?.Name;
            }

            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.FullName, role, shopId);

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = user.Role,
                ShopId = shopId,
                ShopName = shopName,
                Token = token
            };
        }
    }
}