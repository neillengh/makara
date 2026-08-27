using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/tasks/{runId}/stream")]
public class TaskStreamController : ControllerBase
{
    [HttpGet]
    public async Task Stream(string runId, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        await Response.WriteAsync($"data: {{\"runId\":\"{runId}\",\"status\":\"started\"}}\n\n", cancellationToken);

        var progress = 0;
        while (progress < 100 && !cancellationToken.IsCancellationRequested)
        {
            progress += 10;
            await Task.Delay(1000, cancellationToken);
            await Response.WriteAsync($"data: {{\"runId\":\"{runId}\",\"progress\":{progress}}}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync($"data: {{\"runId\":\"{runId}\",\"status\":\"completed\"}}\n\n", cancellationToken);
    }
}
