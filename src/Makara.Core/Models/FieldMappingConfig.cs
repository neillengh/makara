namespace Makara.Core.Models;

/// <summary>
/// 字段映射配置持久化实体（工作流画布中 FieldMap 节点的详细配置）
/// 注意：此表对应的实体类命名为 FieldMappingConfig 以避免与已有的轻量值对象 FieldMapping.cs 冲突
/// </summary>
public class FieldMappingConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WorkflowId { get; set; } = string.Empty;
    public string? SourceDataSourceId { get; set; }

    /// <summary>目标字段 Schema JSON 字符串</summary>
    public string? TargetSchemaJson { get; set; }

    /// <summary>源字段->目标字段映射关系 JSON 字符串</summary>
    public string MappingsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
