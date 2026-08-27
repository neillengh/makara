using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure.DataProcessing;
using Makara.Infrastructure.DataSourceProviders;

namespace Makara.Server.Services;

public class EtlService : IEtlService
{
    private readonly IRepository<DataSource> _dsRepo;
    private readonly DataSourceProviderFactory _providerFactory;
    private readonly DataCleaner _cleaner;
    private readonly IDatasetBuilder _datasetBuilder;

    public EtlService(
        IRepository<DataSource> dsRepo,
        DataSourceProviderFactory providerFactory,
        DataCleaner cleaner,
        IDatasetBuilder datasetBuilder)
    {
        _dsRepo = dsRepo;
        _providerFactory = providerFactory;
        _cleaner = cleaner;
        _datasetBuilder = datasetBuilder;
    }

    public async Task<EtlResult> ExecuteAsync(EtlRequest request)
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            var dataSource = await _dsRepo.GetByIdAsync(request.DataSourceId)
                ?? throw new InvalidOperationException("数据源不存在");

            var provider = _providerFactory.GetProvider(dataSource.Type)
                ?? throw new InvalidOperationException("不支持的数据源类型");

            // 1. 抽取
            var rawData = await provider.ExtractAsync(dataSource, request.IncrementalValue);

            // 2. 清洗
            var cleanData = _cleaner.Clean(rawData, request.DatasetConfig);
            var cleanSummary = _cleaner.GetSummary(
                rawData.Count(), cleanData.Count, request.DatasetConfig);

            // 3. 转换 + 导出
            var datasetResult = await _datasetBuilder.BuildAsync(
                cleanData, request.DatasetConfig, request.OutputDir);

            return new EtlResult
            {
                Success = true,
                CleanSummary = cleanSummary,
                DatasetResult = datasetResult,
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new EtlResult
            {
                Success = false,
                Error = ex.Message,
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<EtlPreview> PreviewAsync(EtlRequest request, int limit = 10)
    {
        var dataSource = await _dsRepo.GetByIdAsync(request.DataSourceId)
            ?? throw new InvalidOperationException("数据源不存在");

        var provider = _providerFactory.GetProvider(dataSource.Type)
            ?? throw new InvalidOperationException("不支持的数据源类型");

        var rawData = await provider.ExtractAsync(dataSource, request.IncrementalValue);
        var cleanData = _cleaner.Clean(rawData, request.DatasetConfig);

        var samples = cleanData
            .Take(limit)
            .Select(row => _datasetBuilder.ConvertRecord(row, request.DatasetConfig))
            .ToList();

        return new EtlPreview
        {
            SampleRecords = samples,
            TotalCount = cleanData.Count,
            CleanSummary = _cleaner.GetSummary(
                rawData.Count(), cleanData.Count, request.DatasetConfig)
        };
    }
}
