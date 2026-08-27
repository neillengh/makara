using Makara.Core.Interfaces;
using Makara.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Makara.Server.Controllers;

/// <summary>
/// 数据集管理 REST API：列表 / 详情 / 样本预览 / 删除
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DatasetsController(IDatasetService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await service.ListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var dataset = await service.GetAsync(id);
        return dataset is null ? NotFound() : Ok(dataset);
    }

    [HttpGet("{id}/samples")]
    public async Task<IActionResult> Samples(string id, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        if (take is < 1 or > 200) take = 50;
        var samples = await service.ListSamplesAsync(id, skip, take);
        return Ok(samples);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
