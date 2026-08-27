using Makara.Core.Enums;

namespace Makara.Desktop.Models;

public class SchedulerStatus
{
    public bool Enabled { get; set; }
    public int ActiveJobCount { get; set; }
    public List<ScheduledJob> Jobs { get; set; } = [];
}

public class ScheduledJob
{
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? NextRunAt { get; set; }
    public bool IsCurrentlyRunning { get; set; }
}
