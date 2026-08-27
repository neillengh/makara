using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure.DataProcessing;

namespace Makara.Server.NodeHandlers;

public class DataCleanNodeHandler : IWorkflowNodeHandler
{
    public string NodeType => "DataClean";

    private readonly DataCleaner _cleaner;

    public DataCleanNodeHandler(DataCleaner cleaner)
    {
        _cleaner = cleaner;
    }

    public Task<object> ExecuteAsync(
        WorkflowNode node,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var rawData = NodeHandlerUtils.GetRowsFromInputs(inputs);
        var config = NodeHandlerUtils.GetDatasetConfig(node);
        var cleanData = _cleaner.Clean(rawData, config);
        return Task.FromResult<object>(cleanData);
    }
}
