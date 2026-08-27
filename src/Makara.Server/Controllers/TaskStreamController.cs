using System.Text.Json;
using Makara.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/tasks/{runId}/stream")]
public class TaskStreamController(IProgressHub progressHub) : ControllerBase
{
    [HttpGet]
    public async Task Stream(string runId, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        await foreach (var evt in progressHub.Subscribe(runId, cancellationToken))
        {
            var json = JsonSerializer.Serialize(evt);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
