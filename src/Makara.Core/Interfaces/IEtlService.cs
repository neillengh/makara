using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IEtlService
{
    Task<EtlResult> ExecuteAsync(EtlRequest request);
    Task<EtlPreview> PreviewAsync(EtlRequest request, int limit = 10);
}

public class EtlRequest
{
    public string DataSourceId { get; set; } = string.Empty;
    public string? IncrementalValue { get; set; }
    public DatasetConfig DatasetConfig { get; set; } = new();
    public string OutputDir { get; set; } = "output";
}

public class EtlResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DataCleanSummary? CleanSummary { get; set; }
    public DatasetResult? DatasetResult { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}

public class EtlPreview
{
    public List<string> SampleRecords { get; set; } = [];
    public int TotalCount { get; set; }
    public DataCleanSummary? CleanSummary { get; set; }
}
