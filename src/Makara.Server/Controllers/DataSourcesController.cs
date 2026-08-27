using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure.DataSourceProviders;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSourcesController(
    IDataSourceService service,
    DataSourceProviderFactory providerFactory) : ControllerBase
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

    [HttpPost("tables")]
    public IActionResult GetTables([FromBody] DataSource dataSource)
    {
        var provider = providerFactory.GetProvider(dataSource.Type);
        if (provider is null) return BadRequest("不支持的数据源类型");
        var tables = provider.GetTableNames(dataSource);
        return Ok(tables);
    }

    [HttpPost("columns")]
    public IActionResult GetColumns([FromBody] DataSource dataSource, [FromQuery] string table)
    {
        var provider = providerFactory.GetProvider(dataSource.Type);
        if (provider is null) return BadRequest("不支持的数据源类型");
        var columns = provider.GetColumnNames(dataSource, table);
        return Ok(columns);
    }
}
