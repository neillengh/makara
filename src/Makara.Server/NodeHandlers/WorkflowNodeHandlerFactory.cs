using Makara.Core.Interfaces;
using Makara.Server.NodeHandlers;

namespace Makara.Server.Services;

public class WorkflowNodeHandlerFactory
{
    private readonly Dictionary<string, IWorkflowNodeHandler> _handlers;

    public WorkflowNodeHandlerFactory(
        DataSourceNodeHandler dataSource,
        DataCleanNodeHandler dataClean,
        FieldMapNodeHandler fieldMap,
        DatasetBuildNodeHandler datasetBuild)
    {
        _handlers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["trigger"] = new PlaceholderNodeHandler("Trigger"),
            ["datasource"] = dataSource,
            ["dataclean"] = dataClean,
            ["fieldmap"] = fieldMap,
            ["datasetbuild"] = datasetBuild,
            ["qualitycheck"] = new PlaceholderNodeHandler("QualityCheck"),
            ["finetune"] = new PlaceholderNodeHandler("Finetune"),
            ["evaluate"] = new PlaceholderNodeHandler("Evaluate"),
            ["deploy"] = new PlaceholderNodeHandler("Deploy"),
            ["notify"] = new PlaceholderNodeHandler("Notify"),
            ["condition"] = new PlaceholderNodeHandler("Condition")
        };
    }

    public IWorkflowNodeHandler? GetHandler(string nodeType) =>
        _handlers.TryGetValue(nodeType, out var handler) ? handler : null;
}
