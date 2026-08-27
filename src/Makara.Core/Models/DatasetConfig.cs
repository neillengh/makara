using Makara.Core.Enums;

namespace Makara.Core.Models;

public class DatasetConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public DatasetFormat OutputFormat { get; set; } = DatasetFormat.QA;
    public FieldMapping Mapping { get; set; } = new();
    public bool MixSyntheticData { get; set; } = false;
    public double SyntheticRatio { get; set; } = 0.3;
    public bool QualityFilter { get; set; } = true;
    public double MinQualityScore { get; set; } = 0.7;
    public bool Dedup { get; set; } = true;
    public double ValSplit { get; set; } = 0.1;
}
