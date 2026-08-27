namespace Makara.Core.Models;

/// <summary>
/// 数据集样本预览（数据集管理页右侧样本面板数据来源）
/// </summary>
public class DatasetSample
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DatasetId { get; set; } = string.Empty;
    public int RecordIndex { get; set; }

    /// <summary>单条样本的 JSON 字符串表示</summary>
    public string JsonData { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
