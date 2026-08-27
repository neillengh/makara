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
}
