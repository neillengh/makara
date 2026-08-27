using Makara.Core.Interfaces;
using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

/// <summary>
/// 认证与用户信息 REST API：登录 / 当前用户 / 用户列表。
/// MVP 简化：无 JWT，Token 即 UserId，客户端保存在本地用于后续拉取用户信息。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service) : ControllerBase
{
    /// <summary>
    /// 用户名密码登录。种子账号：admin / admin123、operator / 123456。
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { success = false, message = "用户名和密码不能为空" });

        var user = await service.LoginAsync(request.Username, request.Password);
        if (user is null)
            return Unauthorized(new { success = false, message = "用户名或密码错误" });

        return Ok(ToLoginResult(user));
    }

    /// <summary>
    /// 获取当前用户信息（客户端启动时用保存的 Token/UserId 拉取）。
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { success = false, message = "缺少 userId 参数" });

        var user = await service.GetUserByIdAsync(userId);
        if (user is null)
            return NotFound(new { success = false, message = "用户不存在" });

        return Ok(new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString()
        });
    }

    private static LoginResult ToLoginResult(User user) => new()
    {
        Success = true,
        Message = "登录成功",
        Token = user.Id,
        User = new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString()
        }
    };
}
