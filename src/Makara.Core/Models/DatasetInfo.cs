using Makara.Core.Enums;

namespace Makara.Core.Models;

/// <summary>
/// 数据集元信息（数据管理页 4 统计卡 + 列表数据由此表驱动）
/// </summary>
public class DatasetInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 存储格式：Jsonl / Parquet / Csv（物理文件格式）
    /// </summary>
    public DatasetStorageFormat StorageFormat { get; set; } = DatasetStorageFormat.Jsonl;

    /// <summary>
    /// 内容格式：QA / Instruction / PlainText / MultiTurn（沿用现有 DatasetFormat 枚举）
    /// </summary>
    public DatasetFormat ContentFormat { get; set; } = DatasetFormat.Instruction;

    public string? SourceWorkflowId { get; set; }
    public string? SourceDataSourceId { get; set; }
    public long SampleCount { get; set; }

    /// <summary>
    /// 字段列表 JSON 字符串，例：["instruction","input","output"]
    /// </summary>
    public string? SchemaJson { get; set; }

    public string? StoragePath { get; set; }

    /// <summary>0-100 质量评分</summary>
    public double QualityScore { get; set; }

    public DatasetStatus Status { get; set; } = DatasetStatus.Building;

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
