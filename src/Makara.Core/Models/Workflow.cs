using Makara.Core.Enums;

namespace Makara.Core.Models;

public class Workflow
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CronExpression { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public List<WorkflowNode> Nodes { get; set; } = [];
    public List<WorkflowEdge> Edges { get; set; } = [];

    /// <summary>节点集合的 JSON 持久化（FreeSql 3.5 不支持 List&lt;T&gt; 直接 JSON 映射，故手动序列化到此字符串列）。</summary>
    public string? NodesJson { get; set; }
    /// <summary>边集合的 JSON 持久化。</summary>
    public string? EdgesJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkflowNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public Dictionary<string, object> Config { get; set; } = new();
}

public class WorkflowEdge
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string? Label { get; set; }
}
