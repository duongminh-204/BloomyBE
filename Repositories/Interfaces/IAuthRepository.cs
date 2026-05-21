using Bloomy.Models;

namespace Bloomy.Data.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phoneNumber);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> IsEmailExistAsync(string email);
        Task<bool> IsPhoneExistAsync(string phoneNumber);
        Task CreateAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
    }
}