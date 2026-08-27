using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

/// <summary>
/// 服务端根级元信息接口：健康检查、版本号等。
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly IFreeSql _fsql;
    private readonly IWebHostEnvironment _env;

    public SystemController(IFreeSql fsql, IWebHostEnvironment env)
    {
        _fsql = fsql;
        _env = env;
    }

    /// <summary>
    /// 服务端健康检查（登录窗口、服务端管理页连接测试使用）。
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        string dbStatus;
        try
        {
            // 用一个低成本查询验证数据库可读，用 User 表任意记录数（即使 0 也说明 DB 正常）
            _ = await _fsql.Select<AuditLog>().CountAsync(cancellationToken);
            dbStatus = "ok";
        }
        catch (Exception ex)
        {
            dbStatus = "error: " + ex.Message;
        }

        return Ok(new
        {
            ok = dbStatus == "ok",
            version = "1.0.0",
            environment = _env.EnvironmentName,
            time = DateTime.UtcNow,
            db = dbStatus
        });
    }
}
