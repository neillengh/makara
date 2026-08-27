using System.Text.Json.Serialization;

namespace Makara.Core.Models;

public class WorkflowEvent
{
    public string RunId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? NodeId { get; set; }
    public string? Message { get; set; }
    public double Progress { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
