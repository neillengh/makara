using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IRepository<Workflow> _repo;
    private readonly IRepository<WorkflowRun> _runRepo;

    public WorkflowService(IRepository<Workflow> repo, IRepository<WorkflowRun> runRepo)
    {
        _repo = repo;
        _runRepo = runRepo;
    }

    public async Task<IEnumerable<Workflow>> ListAsync() =>
        await _repo.GetAllAsync();

    public async Task<Workflow?> GetAsync(string id) =>
        await _repo.GetByIdAsync(id);

    public async Task<Workflow> CreateAsync(Workflow workflow) =>
        await _repo.InsertAsync(workflow);

    public async Task<Workflow> UpdateAsync(string id, Workflow workflow)
    {
        workflow.Id = id;
        workflow.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(workflow);
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task<string> RunAsync(string workflowId)
    {
        var run = new WorkflowRun
        {
            WorkflowId = workflowId,
            Status = "queued",
            StartedAt = DateTime.UtcNow
        };
        await _runRepo.InsertAsync(run);
        // TODO: 触发工作流执行引擎
        return run.Id;
    }

    public async Task<WorkflowRun?> GetRunStatusAsync(string runId) =>
        await _runRepo.GetByIdAsync(runId);
}
