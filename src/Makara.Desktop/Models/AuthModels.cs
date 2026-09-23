namespace Makara.Desktop.Models;

/// <summary>
/// 登录返回的用户信息（对应服务端 AuthController 的 UserInfo）
/// </summary>
public class AuthUser
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public string Role { get; set; } = "";
}

/// <summary>
/// 登录接口返回体：{ success, message, token, user }
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string Token { get; set; } = "";
    public AuthUser? User { get; set; }
}
