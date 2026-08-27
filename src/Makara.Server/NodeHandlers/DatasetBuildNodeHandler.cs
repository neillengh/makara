using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.NodeHandlers;

public class DatasetBuildNodeHandler : IWorkflowNodeHandler
{
    public string NodeType => "DatasetBuild";

    private readonly IDatasetBuilder _datasetBuilder;

    public DatasetBuildNodeHandler(IDatasetBuilder datasetBuilder)
    {
        _datasetBuilder = datasetBuilder;
    }

    public async Task<object> ExecuteAsync(
        WorkflowNode node,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var rows = NodeHandlerUtils.GetRowsFromInputs(inputs);
        var config = NodeHandlerUtils.GetDatasetConfig(node);
        var outputDir = NodeHandlerUtils.GetConfigString(node, "outputDir");

        if (string.IsNullOrEmpty(outputDir))
            outputDir = "output";

        return await _datasetBuilder.BuildAsync(rows, config, outputDir);
    }
}
