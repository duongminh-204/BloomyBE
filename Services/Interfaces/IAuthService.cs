namespace BloomyBE.Services.Interfaces
{
  
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> ExternalLoginAsync(string provider, string providerKey, string email, string fullName);
    }
}
