using System.Text.Json;
using Makara.Core.Models;

namespace Makara.Server.NodeHandlers;

public static class NodeHandlerUtils
{
    public static List<Dictionary<string, object>> GetRowsFromInputs(
        Dictionary<string, object> inputs)
    {
        foreach (var value in inputs.Values)
        {
            if (value is List<Dictionary<string, object>> rows)
                return rows;
        }
        return [];
    }

    public static DatasetConfig GetDatasetConfig(WorkflowNode node)
    {
        if (node.Config.TryGetValue("datasetConfig", out var v))
        {
            if (v is JsonElement el)
                return el.Deserialize<DatasetConfig>() ?? new DatasetConfig();
            if (v is DatasetConfig cfg)
                return cfg;
        }
        return new DatasetConfig();
    }

    public static string GetConfigString(WorkflowNode node, string key)
    {
        if (node.Config.TryGetValue(key, out var v))
            return v?.ToString() ?? string.Empty;
        return string.Empty;
    }
}
