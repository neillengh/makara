namespace Makara.Core.Models;

public class WorkflowRun
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WorkflowId { get; set; } = string.Empty;
    public string Status { get; set; } = "queued";
    public double Progress { get; set; }
    public string? CurrentNode { get; set; }
    public string? Result { get; set; }
    public string? Error { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public List<RunLog> Logs { get; set; } = [];

    /// <summary>日志集合的 JSON 持久化（FreeSql 3.5 不支持 List&lt;T&gt; 直接 JSON 映射，故手动序列化到此字符串列）。</summary>
    public string? LogsJson { get; set; }
}

public class RunLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string RunId { get; set; } = string.Empty;
    public string Level { get; set; } = "info";
    public string Message { get; set; } = string.Empty;
    public string? NodeId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
