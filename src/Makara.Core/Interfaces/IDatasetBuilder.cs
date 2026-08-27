using Makara.Core.Enums;
using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IDatasetBuilder
{
    Task<DatasetResult> BuildAsync(
        IEnumerable<Dictionary<string, object>> rawData,
        DatasetConfig config,
        string outputDir);
}

public class DatasetResult
{
    public string TrainPath { get; set; } = string.Empty;
    public string ValPath { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int TrainCount { get; set; }
    public int ValCount { get; set; }
}
