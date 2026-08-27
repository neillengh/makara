using System.Text.Json;
using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Server.Services;

namespace Makara.Server.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly WorkflowNodeHandlerFactory _handlerFactory;
    private readonly IProgressHub _progressHub;
    private readonly IRepository<WorkflowRun> _runRepo;

    public WorkflowEngine(
        WorkflowNodeHandlerFactory handlerFactory,
        IProgressHub progressHub,
        IRepository<WorkflowRun> runRepo)
    {
        _handlerFactory = handlerFactory;
        _progressHub = progressHub;
        _runRepo = runRepo;
    }

    public async Task<string> RunAsync(Workflow workflow)
    {
        var run = new WorkflowRun
        {
            WorkflowId = workflow.Id,
            Status = "running",
            StartedAt = DateTime.UtcNow
        };
        await _runRepo.InsertAsync(run);

        _ = ExecuteWorkflowAsync(run, workflow);

        return run.Id;
    }

    public async Task CancelAsync(string runId)
    {
        var run = await _runRepo.GetByIdAsync(runId);
        if (run is null) return;

        run.Status = "cancelled";
        run.FinishedAt = DateTime.UtcNow;
        await _runRepo.UpdateAsync(run);

        _progressHub.Publish(new WorkflowEvent
        {
            RunId = runId,
            Type = "cancelled",
            Message = "工作流已取消"
        });
        _progressHub.Complete(runId);
    }

    public async Task<WorkflowRunStatus> GetStatusAsync(string runId)
    {
        var run = await _runRepo.GetByIdAsync(runId)
            ?? throw new InvalidOperationException("运行记录不存在");

        return new WorkflowRunStatus
        {
            RunId = run.Id,
            Status = run.Status,
            Progress = run.Progress,
            CurrentNode = run.CurrentNode,
            Logs = run.Logs.Select(l => l.Message).ToList(),
            Result = run.Result,
            StartedAt = run.StartedAt,
            FinishedAt = run.FinishedAt
        };
    }

    private async Task ExecuteWorkflowAsync(WorkflowRun run, Workflow workflow)
    {
        var nodeOutputs = new Dictionary<string, object>();
        var logs = new List<RunLog>();

        try
        {
            var orderedNodes = TopologicalSort(workflow.Nodes, workflow.Edges);
            var totalNodes = orderedNodes.Count;

            Publish(run.Id, "started", $"工作流 {workflow.Name} 开始执行", 0);

            for (var i = 0; i < totalNodes; i++)
            {
                var node = orderedNodes[i];
                run.CurrentNode = node.Label;
                run.Progress = (double)i / totalNodes * 100;
                await _runRepo.UpdateAsync(run);

                Publish(run.Id, "node_start", $"开始执行: {node.Label}",
                    run.Progress, node.Id);

                var handler = _handlerFactory.GetHandler(node.Type)
                    ?? throw new InvalidOperationException($"未知节点类型: {node.Type}");

                var inputs = new Dictionary<string, object>();
                foreach (var edge in workflow.Edges.Where(e => e.TargetNodeId == node.Id))
                {
                    if (nodeOutputs.TryGetValue(edge.SourceNodeId, out var upstreamOutput))
                        inputs[edge.SourceNodeId] = upstreamOutput;
                }

                var output = await handler.ExecuteAsync(node, inputs);
                nodeOutputs[node.Id] = output;

                logs.Add(new RunLog
                {
                    RunId = run.Id,
                    Level = "info",
                    Message = $"节点 {node.Label} 执行完成",
                    NodeId = node.Id
                });

                run.Progress = (double)(i + 1) / totalNodes * 100;
                await _runRepo.UpdateAsync(run);

                Publish(run.Id, "node_complete", $"节点 {node.Label} 执行完成",
                    run.Progress, node.Id);
            }

            run.Status = "succeeded";
            run.Progress = 100;
            run.Logs = logs;
            run.LogsJson = JsonSerializer.Serialize(logs, GraphJsonColumns.JsonOptions);
            run.FinishedAt = DateTime.UtcNow;
            await _runRepo.UpdateAsync(run);

            Publish(run.Id, "completed", "工作流执行完成", 100);
        }
        catch (Exception ex)
        {
            run.Status = "failed";
            run.Error = ex.Message;
            run.Logs = logs;
            run.LogsJson = JsonSerializer.Serialize(logs, GraphJsonColumns.JsonOptions);
            run.FinishedAt = DateTime.UtcNow;
            await _runRepo.UpdateAsync(run);

            Publish(run.Id, "error", ex.Message, run.Progress);
        }
        finally
        {
            _progressHub.Complete(run.Id);
        }
    }

    private void Publish(
        string runId, string type, string message,
        double progress, string? nodeId = null)
    {
        _progressHub.Publish(new WorkflowEvent
        {
            RunId = runId,
            Type = type,
            Message = message,
            Progress = progress,
            NodeId = nodeId
        });
    }

    private static List<WorkflowNode> TopologicalSort(
        List<WorkflowNode> nodes, List<WorkflowEdge> edges)
    {
        var inDegree = nodes.ToDictionary(n => n.Id, _ => 0);
        var adjacency = nodes.ToDictionary(n => n.Id, _ => new List<string>());

        foreach (var edge in edges)
        {
            if (adjacency.ContainsKey(edge.SourceNodeId))
                adjacency[edge.SourceNodeId].Add(edge.TargetNodeId);
            if (inDegree.ContainsKey(edge.TargetNodeId))
                inDegree[edge.TargetNodeId]++;
        }

        var queue = new Queue<string>(
            inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));

        var result = new List<WorkflowNode>();
        var nodeMap = nodes.ToDictionary(n => n.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (nodeMap.TryGetValue(current, out var node))
                result.Add(node);

            foreach (var neighbor in adjacency[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                    queue.Enqueue(neighbor);
            }
        }

        if (result.Count != nodes.Count)
            throw new InvalidOperationException("工作流中存在循环依赖");

        return result;
    }
}
