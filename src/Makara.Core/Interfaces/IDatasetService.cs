using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IDatasetService
{
    Task<IEnumerable<DatasetInfo>> ListAsync();
    Task<DatasetInfo?> GetAsync(string id);
    Task<DatasetInfo> CreateAsync(DatasetInfo dataset);
    Task<DatasetInfo> UpdateAsync(string id, DatasetInfo dataset);
    Task<bool> DeleteAsync(string id);

    Task<List<DatasetSample>> ListSamplesAsync(string datasetId, int skip = 0, int take = 50);

    /// <summary>
    /// 预览字段映射结果（用于工作流画布字段映射节点的"预览映射"按钮）
    /// </summary>
    Task<MappingPreview> PreviewFieldMappingAsync(FieldMappingConfig mapping, int limit = 10);
}

public class MappingPreview
{
    public List<string> SourceFields { get; set; } = [];
    public List<string> TargetFields { get; set; } = [];
    public List<Dictionary<string, object?>> Records { get; set; } = [];
    public int TotalCount { get; set; }
}
