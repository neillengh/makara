using Makara.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/scheduler")]
public class SchedulerController(WorkflowSchedulerService scheduler) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var jobs = await scheduler.GetScheduledJobsAsync();
        return Ok(new
        {
            enabled = scheduler.IsEnabled,
            activeJobCount = jobs.Count,
            jobs
        });
    }

    [HttpPost("enable")]
    public IActionResult Enable()
    {
        scheduler.Enable();
        return Ok(new { enabled = true });
    }

    [HttpPost("disable")]
    public IActionResult Disable()
    {
        scheduler.Disable();
        return Ok(new { enabled = false });
    }
}
