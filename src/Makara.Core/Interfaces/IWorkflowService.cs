using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IWorkflowService
{
    Task<IEnumerable<Workflow>> ListAsync();
    Task<Workflow?> GetAsync(string id);
    Task<Workflow> CreateAsync(Workflow workflow);
    Task<Workflow> UpdateAsync(string id, Workflow workflow);
    Task<bool> DeleteAsync(string id);
    Task<string> RunAsync(string workflowId);
    Task<WorkflowRun?> GetRunStatusAsync(string runId);

    /// <summary>按开始时间倒序查询运行记录（供客户端执行记录页/仪表盘展示）。</summary>
    Task<List<WorkflowRun>> ListRunsAsync(int take = 100);
}
