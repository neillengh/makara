namespace Makara.Core.Models;

/// <summary>
/// 服务端级系统设置（Key/Value 结构，给设置页的默认参数/数据安全两组使用）
/// </summary>
public class SystemSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
