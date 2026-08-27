using Makara.Core.Interfaces;
using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(IWorkflowService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await service.ListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var wf = await service.GetAsync(id);
        return wf is null ? NotFound() : Ok(wf);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Workflow workflow)
    {
        var created = await service.CreateAsync(workflow);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Workflow workflow)
    {
        await service.UpdateAsync(id, workflow);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/run")]
    public async Task<IActionResult> Run(string id)
    {
        var runId = await service.RunAsync(id);
        return Ok(new { runId, workflowId = id, status = "queued" });
    }

    [HttpGet("runs/{runId}/status")]
    public async Task<IActionResult> GetRunStatus(string runId)
    {
        var run = await service.GetRunStatusAsync(runId);
        return run is null ? NotFound() : Ok(run);
    }

    /// <summary>运行记录列表（按开始时间倒序），供客户端执行记录页与仪表盘展示。</summary>
    [HttpGet("runs")]
    public async Task<IActionResult> ListRuns([FromQuery] int take = 100)
    {
        if (take is < 1 or > 500) take = 100;
        var runs = await service.ListRunsAsync(take);
        return Ok(runs);
    }
}
