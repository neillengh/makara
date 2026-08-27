using System.Security.Cryptography;
using System.Text;
using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _repo;

    public AuthService(IRepository<User> repo)
    {
        _repo = repo;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var users = await _repo.GetAllAsync(u => u.Username == username);
        var user = users.FirstOrDefault();
        if (user is null) return null;

        var expected = HashPassword(password);
        if (!string.Equals(user.PasswordHash, expected, StringComparison.Ordinal))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(user);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(string userId) =>
        await _repo.GetByIdAsync(userId);

    private static string HashPassword(string pwd)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pwd + "makara-salt-v1"));
        return Convert.ToBase64String(bytes);
    }
}
