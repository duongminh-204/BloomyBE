using Bloomy.Data.Interfaces;
using Bloomy.DTOs.Auth;
using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.Services.Interfaces;
using BloomyBE.Services;

namespace Bloomy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(IAuthRepository authRepo, IJwtTokenService jwtTokenService)
        {
            _authRepo = authRepo;
            _jwtTokenService = jwtTokenService;
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

            return CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetByEmailAsync(dto.EmailOrPhone);

            if (user == null)
                user = await _authRepo.GetByPhoneAsync(dto.EmailOrPhone);

            if (user == null || !await _authRepo.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Email/Số điện thoại hoặc mật khẩu không chính xác.");

            return CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _authRepo.GetByIdAsync(userId);
            return user == null ? null : CreateAuthResponse(user);
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var role = user.Role switch
            {
                UserRole.ShopOwner => "ShopOwner",
                UserRole.Admin => "Admin",
                _ => "Customer"
            };

            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.FullName, role);

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = user.Role,
                Token = token
            };
        }
    }
}