namespace Makara.Desktop.Models;

/// <summary>
/// 当前会话（登录后保存 token 与用户信息，供各页面读取）
/// </summary>
public static class Session
{
    public static string Token { get; set; } = "";
    public static AuthUser? CurrentUser { get; set; }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public static void Clear()
    {
        Token = "";
        CurrentUser = null;
    }
}
