namespace Makara.Core.Models;

/// <summary>
/// 审计日志（MVP 阶段只 Seed 不做 UI，后续扩展用）
/// </summary>
public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
