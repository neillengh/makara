using Makara.Core.Interfaces;
using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/etl")]
public class EtlController(IEtlService etlService) : ControllerBase
{
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] EtlRequest request) =>
        Ok(await etlService.ExecuteAsync(request));

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] EtlRequest request, [FromQuery] int limit = 10) =>
        Ok(await etlService.PreviewAsync(request, limit));
}
