using Makara.Core.Interfaces;
using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSourcesController(IDataSourceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await service.ListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var ds = await service.GetAsync(id);
        return ds is null ? NotFound() : Ok(ds);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DataSource dataSource)
    {
        var created = await service.CreateAsync(dataSource);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] DataSource dataSource)
    {
        await service.UpdateAsync(id, dataSource);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] DataSource dataSource)
    {
        var connected = await service.TestConnectionAsync(dataSource);
        return Ok(new { connected });
    }
}
