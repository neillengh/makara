using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.NodeHandlers;

public class PlaceholderNodeHandler : IWorkflowNodeHandler
{
    public string NodeType { get; }

    public PlaceholderNodeHandler(string nodeType)
    {
        NodeType = nodeType;
    }

    public Task<object> ExecuteAsync(
        WorkflowNode node,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var rows = NodeHandlerUtils.GetRowsFromInputs(inputs);
        return Task.FromResult<object>(rows);
    }
}
