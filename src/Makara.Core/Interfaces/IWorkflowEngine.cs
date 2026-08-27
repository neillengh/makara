using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IWorkflowEngine
{
    Task<string> RunAsync(Workflow workflow);
    Task CancelAsync(string runId);
    Task<WorkflowRunStatus> GetStatusAsync(string runId);
}

public class WorkflowRunStatus
{
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Progress { get; set; }
    public string? CurrentNode { get; set; }
    public List<string> Logs { get; set; } = [];
    public string? Result { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
