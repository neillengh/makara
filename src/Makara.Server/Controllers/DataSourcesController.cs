using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSourcesController : ControllerBase
{
    [HttpGet]
    public IActionResult List()
    {
        return Ok(new List<DataSource>());
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        return Ok(new { id });
    }

    [HttpPost]
    public IActionResult Create([FromBody] DataSource dataSource)
    {
        return CreatedAtAction(nameof(Get), new { id = dataSource.Id }, dataSource);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] DataSource dataSource)
    {
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        return NoContent();
    }

    [HttpPost("test")]
    public IActionResult TestConnection([FromBody] DataSource dataSource)
    {
        return Ok(new { connected = true });
    }
}
