using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IAuthService
{
    /// <summary>用户名+密码登录，成功返回用户实体，失败返回 null。</summary>
    Task<User?> LoginAsync(string username, string password);

    /// <summary>按 ID 获取用户（用于 api/auth/me）。</summary>
    Task<User?> GetUserByIdAsync(string userId);
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; } // MVP 简化：可放 UserId；后续升级为 JWT
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "User";
}
