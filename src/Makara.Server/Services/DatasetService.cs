using System.Text.Json;
using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class DatasetService : IDatasetService
{
    private readonly IRepository<DatasetInfo> _repo;
    private readonly IRepository<DatasetSample> _sampleRepo;

    public DatasetService(
        IRepository<DatasetInfo> repo,
        IRepository<DatasetSample> sampleRepo)
    {
        _repo = repo;
        _sampleRepo = sampleRepo;
    }

    public async Task<IEnumerable<DatasetInfo>> ListAsync() =>
        await _repo.GetAllAsync(null, 0, 1000, nameof(DatasetInfo.UpdatedAt), true);

    public async Task<DatasetInfo?> GetAsync(string id) =>
        await _repo.GetByIdAsync(id);

    public async Task<DatasetInfo> CreateAsync(DatasetInfo dataset) =>
        await _repo.InsertAsync(dataset);

    public async Task<DatasetInfo> UpdateAsync(string id, DatasetInfo dataset)
    {
        dataset.Id = id;
        dataset.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(dataset);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // 先把样表级联删
        var samples = await _sampleRepo.GetAllAsync(s => s.DatasetId == id);
        foreach (var s in samples)
            await _sampleRepo.DeleteAsync(s.Id);

        return await _repo.DeleteAsync(id);
    }

    public async Task<List<DatasetSample>> ListSamplesAsync(string datasetId, int skip = 0, int take = 50) =>
        await _sampleRepo.GetAllAsync(
            s => s.DatasetId == datasetId,
            skip, take,
            nameof(DatasetSample.RecordIndex),
            false);

    public Task<MappingPreview> PreviewFieldMappingAsync(FieldMappingConfig mapping, int limit = 10)
    {
        // MVP：按映射配置构造一份确定性的预览（不从真实数据源拉），供 UI 可视化确认
        var source = new List<string>();
        var target = new List<string>();
        try
        {
            if (!string.IsNullOrWhiteSpace(mapping.TargetSchemaJson))
                target = JsonSerializer.Deserialize<List<string>>(mapping.TargetSchemaJson) ?? [];
            var mappings = string.IsNullOrWhiteSpace(mapping.MappingsJson)
                ? new List<Dictionary<string, object?>>()
                : JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(mapping.MappingsJson) ?? [];
            source.AddRange(mappings.Select(m =>
                m.TryGetValue("source", out var s) ? s?.ToString() ?? "(null)" : "(source?)"));
        }
        catch
        {
            // 预览容错：格式异常时给空列表
        }

        if (source.Count == 0) source = new List<string> { "src_field1", "src_field2", "src_field3" };
        if (target.Count == 0) target = new List<string> { "instruction", "input", "output" };

        var records = new List<Dictionary<string, object?>>();
        for (int i = 0; i < limit; i++)
        {
            var record = new Dictionary<string, object?>();
            for (int j = 0; j < target.Count; j++)
            {
                var src = source.Count > j ? source[j] : $"col{j}";
                record[target[j]] = $"<{src}>_样本值_{i}";
            }
            records.Add(record);
        }

        return Task.FromResult(new MappingPreview
        {
            SourceFields = source,
            TargetFields = target,
            Records = records,
            TotalCount = 1000 + limit
        });
    }
}
