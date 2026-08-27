using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IRepository<Workflow> _repo;
    private readonly IRepository<WorkflowRun> _runRepo;
    private readonly IWorkflowEngine _engine;

    public WorkflowService(
        IRepository<Workflow> repo,
        IRepository<WorkflowRun> runRepo,
        IWorkflowEngine engine)
    {
        _repo = repo;
        _runRepo = runRepo;
        _engine = engine;
    }

    public async Task<IEnumerable<Workflow>> ListAsync()
    {
        var list = await _repo.GetAllAsync();
        foreach (var wf in list) GraphJsonColumns.Hydrate(wf);
        return list;
    }

    public async Task<Workflow?> GetAsync(string id)
    {
        var wf = await _repo.GetByIdAsync(id);
        if (wf is not null) GraphJsonColumns.Hydrate(wf);
        return wf;
    }

    public async Task<Workflow> CreateAsync(Workflow workflow)
    {
        workflow.UpdatedAt = DateTime.UtcNow;
        GraphJsonColumns.Dehydrate(workflow);
        return await _repo.InsertAsync(workflow);
    }

    public async Task<Workflow> UpdateAsync(string id, Workflow workflow)
    {
        workflow.Id = id;
        workflow.UpdatedAt = DateTime.UtcNow;
        GraphJsonColumns.Dehydrate(workflow);
        return await _repo.UpdateAsync(workflow);
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task<string> RunAsync(string workflowId)
    {
        var workflow = await _repo.GetByIdAsync(workflowId)
            ?? throw new InvalidOperationException("工作流不存在");
        GraphJsonColumns.Hydrate(workflow);

        return await _engine.RunAsync(workflow);
    }

    public async Task<WorkflowRun?> GetRunStatusAsync(string runId)
    {
        var run = await _runRepo.GetByIdAsync(runId);
        if (run is not null) GraphJsonColumns.Hydrate(run);
        return run;
    }

    public async Task<List<WorkflowRun>> ListRunsAsync(int take = 100)
    {
        var runs = await _runRepo.GetAllAsync(
            predicate: null,
            skip: 0,
            take: Math.Clamp(take, 1, 500),
            orderByKey: nameof(WorkflowRun.StartedAt),
            descending: true);
        foreach (var r in runs) GraphJsonColumns.Hydrate(r);
        return runs;
    }
}
