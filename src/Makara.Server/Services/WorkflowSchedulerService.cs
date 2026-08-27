using System.Collections.Concurrent;
using Cronos;
using Makara.Core.Enums;
using Makara.Core.Interfaces;
using Makara.Core.Models;
using Microsoft.Extensions.Hosting;

namespace Makara.Server.Services;

public class WorkflowSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowSchedulerService> _logger;

    private volatile bool _enabled = true;
    private readonly ConcurrentDictionary<string, bool> _runningWorkflows = new();
    private readonly ConcurrentDictionary<string, DateTime> _nextRunTimes = new();

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    public WorkflowSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<WorkflowSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public bool IsEnabled => _enabled;

    public void Enable() => _enabled = true;
    public void Disable() => _enabled = false;

    public async Task<List<ScheduledJobInfo>> GetScheduledJobsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Workflow>>();
        var workflows = await repo.GetAllAsync();

        return workflows
            .Where(w => !string.IsNullOrEmpty(w.CronExpression))
            .Select(w => new ScheduledJobInfo
            {
                WorkflowId = w.Id,
                WorkflowName = w.Name,
                CronExpression = w.CronExpression!,
                Status = w.Status.ToString(),
                NextRunAt = _nextRunTimes.TryGetValue(w.Id, out var next) ? next : null,
                IsCurrentlyRunning = _runningWorkflows.ContainsKey(w.Id)
            })
            .ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("工作流调度服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_enabled)
            {
                try
                {
                    await CheckAndRunWorkflows(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "调度器检查失败");
                }
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException) { }
        }

        _logger.LogInformation("工作流调度服务已停止");
    }

    private async Task CheckAndRunWorkflows(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Workflow>>();
        var engine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();

        var workflows = await repo.GetAllAsync();
        var scheduled = workflows
            .Where(w => !string.IsNullOrEmpty(w.CronExpression)
                && w.Status == WorkflowStatus.Ready);

        var now = DateTime.UtcNow;

        foreach (var workflow in scheduled)
        {
            if (ct.IsCancellationRequested) break;
            if (_runningWorkflows.ContainsKey(workflow.Id)) continue;

            if (!_nextRunTimes.TryGetValue(workflow.Id, out var nextRun))
            {
                var calculated = CalculateNextRun(workflow.CronExpression!, now);
                if (!calculated.HasValue) continue;
                nextRun = calculated.Value;
                _nextRunTimes[workflow.Id] = nextRun;
            }

            if (nextRun <= now)
            {
                _runningWorkflows[workflow.Id] = true;
                _logger.LogInformation("触发工作流: {Name} ({Id})", workflow.Name, workflow.Id);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await engine.RunAsync(workflow);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "工作流 {Id} 执行失败", workflow.Id);
                    }
                    finally
                    {
                        _runningWorkflows.TryRemove(workflow.Id, out _);
                        var futureRun = CalculateNextRun(workflow.CronExpression!, DateTime.UtcNow);
                        if (futureRun.HasValue)
                            _nextRunTimes[workflow.Id] = futureRun.Value;
                    }
                }, ct);
            }
        }
    }

    private static DateTime? CalculateNextRun(string cronExpression, DateTime from)
    {
        try
        {
            var cron = CronExpression.Parse(cronExpression);
            return cron.GetNextOccurrence(from, TimeZoneInfo.Utc);
        }
        catch
        {
            return null;
        }
    }
}

public class ScheduledJobInfo
{
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? NextRunAt { get; set; }
    public bool IsCurrentlyRunning { get; set; }
}
