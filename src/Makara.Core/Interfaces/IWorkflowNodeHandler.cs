using Makara.Core.Models;

namespace Makara.Core.Interfaces;

public interface IWorkflowNodeHandler
{
    string NodeType { get; }

    Task<object> ExecuteAsync(
        WorkflowNode node,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default);
}
