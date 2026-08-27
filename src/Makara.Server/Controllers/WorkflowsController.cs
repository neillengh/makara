using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    [HttpGet]
    public IActionResult List()
    {
        return Ok(new List<Workflow>());
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        return Ok(new { id });
    }

    [HttpPost]
    public IActionResult Create([FromBody] Workflow workflow)
    {
        return CreatedAtAction(nameof(Get), new { id = workflow.Id }, workflow);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Workflow workflow)
    {
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        return NoContent();
    }

    [HttpPost("{id}/run")]
    public IActionResult Run(string id)
    {
        var runId = Guid.NewGuid().ToString("N");
        return Ok(new { runId, workflowId = id, status = "queued" });
    }

    [HttpGet("runs/{runId}/status")]
    public IActionResult GetRunStatus(string runId)
    {
        return Ok(new { runId, status = "running", progress = 0 });
    }
}
