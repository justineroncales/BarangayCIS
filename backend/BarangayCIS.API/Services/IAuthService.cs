using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string email, string password, string fullName, string role);
        Task<User?> GetUserByIdAsync(int userId);
    }
}


