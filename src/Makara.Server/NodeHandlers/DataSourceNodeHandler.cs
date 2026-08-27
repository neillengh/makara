using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure.DataSourceProviders;

namespace Makara.Server.NodeHandlers;

public class DataSourceNodeHandler : IWorkflowNodeHandler
{
    public string NodeType => "DataSource";

    private readonly IRepository<DataSource> _dsRepo;
    private readonly DataSourceProviderFactory _providerFactory;

    public DataSourceNodeHandler(
        IRepository<DataSource> dsRepo,
        DataSourceProviderFactory providerFactory)
    {
        _dsRepo = dsRepo;
        _providerFactory = providerFactory;
    }

    public async Task<object> ExecuteAsync(
        WorkflowNode node,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var dataSourceId = NodeHandlerUtils.GetConfigString(node, "dataSourceId");
        var dataSource = await _dsRepo.GetByIdAsync(dataSourceId)
            ?? throw new InvalidOperationException($"数据源 {dataSourceId} 不存在");

        var provider = _providerFactory.GetProvider(dataSource.Type)
            ?? throw new InvalidOperationException($"不支持的数据源类型 {dataSource.Type}");

        var incrementalValue = node.Config.TryGetValue("incrementalValue", out var v)
            ? v?.ToString() : null;

        var rawData = await provider.ExtractAsync(dataSource, incrementalValue);
        return rawData.ToList();
    }
}
