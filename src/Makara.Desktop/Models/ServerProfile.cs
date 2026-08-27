namespace Makara.Desktop.Models;

/// <summary>
/// 服务端连接配置（服务端管理页数据模型，持久化到本地 JSON）
/// </summary>
public class ServerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "本地开发";
    public string Address { get; set; } = "http://localhost:5000";
    public string Description { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public override string ToString() => Name;
}
