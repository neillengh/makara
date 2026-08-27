namespace Makara.Core.Models;

public class DataCleanSummary
{
    public int RawCount { get; set; }
    public int CleanedCount { get; set; }
    public int RemovedCount { get; set; }
    public bool DedupEnabled { get; set; }
    public bool QualityFilterEnabled { get; set; }
}
