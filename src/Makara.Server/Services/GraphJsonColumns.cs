using System.Text.Json;
using Makara.Core.Models;

namespace Makara.Server.Services;

/// <summary>
/// 在 FreeSql 实体（List&lt;T&gt; 集合属性）与持久化用的 *Json 字符串列之间互相转换。
/// FreeSql 3.5 不支持直接把 List&lt;T&gt; 映射为 JSON 列，因此采用字符串列手动序列化方案。
/// </summary>
internal static class GraphJsonColumns
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Hydrate(Workflow w)
    {
        w.Nodes = Read<List<WorkflowNode>>(w.NodesJson);
        w.Edges = Read<List<WorkflowEdge>>(w.EdgesJson);
    }

    public static void Dehydrate(Workflow w)
    {
        w.NodesJson = Write(w.Nodes);
        w.EdgesJson = Write(w.Edges);
    }

    public static void Hydrate(WorkflowRun r)
    {
        r.Logs = Read<List<RunLog>>(r.LogsJson);
    }

    public static void Dehydrate(WorkflowRun r)
    {
        r.LogsJson = Write(r.Logs);
    }

    private static T Read<T>(string? json) where T : class, new() =>
        string.IsNullOrWhiteSpace(json)
            ? new T()
            : JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();

    private static string Write<T>(T? value) =>
        JsonSerializer.Serialize((object?)value ?? new List<object>(), JsonOptions);
}
