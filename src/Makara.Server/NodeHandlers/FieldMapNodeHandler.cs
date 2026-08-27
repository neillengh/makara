using System.Text.Json;
using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.NodeHandlers;

public class FieldMapNodeHandler : IWorkflowNodeHandler
{
    public string NodeType => "FieldMap";

    public Task<object> ExecuteAsync(
        WorkflowNode node,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var rows = NodeHandlerUtils.GetRowsFromInputs(inputs);
        var mapping = GetMapping(node);

        var result = rows.Select(row =>
        {
            var mapped = new Dictionary<string, object>();
            foreach (var kvp in row)
            {
                var newName = mapping.TryGetValue(kvp.Key, out var renamed)
                    ? renamed : kvp.Key;
                mapped[newName] = kvp.Value;
            }
            return mapped;
        }).ToList();

        return Task.FromResult<object>(result);
    }

    private static Dictionary<string, string> GetMapping(WorkflowNode node)
    {
        if (node.Config.TryGetValue("mapping", out var v) && v is JsonElement el)
        {
            return el.Deserialize<Dictionary<string, string>>() ?? [];
        }
        return [];
    }
}
