using System.Text.Json;
using Makara.Core.Models;

namespace Makara.Infrastructure.DataProcessing;

public class DataCleaner
{
    public List<Dictionary<string, object>> Clean(
        IEnumerable<Dictionary<string, object>> rawData,
        DatasetConfig config)
    {
        var data = rawData.ToList();

        // 过滤空值
        data = FilterNulls(data, config);

        // 去重
        if (config.Dedup)
            data = Dedup(data, config);

        // 格式标准化
        data = Standardize(data);

        // 质量过滤
        if (config.QualityFilter)
            data = QualityFilter(data, config.MinQualityScore);

        return data;
    }

    public DataCleanSummary GetSummary(
        int rawCount, int cleanedCount, DatasetConfig config)
    {
        return new DataCleanSummary
        {
            RawCount = rawCount,
            CleanedCount = cleanedCount,
            RemovedCount = rawCount - cleanedCount,
            DedupEnabled = config.Dedup,
            QualityFilterEnabled = config.QualityFilter
        };
    }

    private static List<Dictionary<string, object>> FilterNulls(
        List<Dictionary<string, object>> data, DatasetConfig config)
    {
        var keyFields = new[] { config.Mapping.QuestionField, config.Mapping.AnswerField }
            .Where(f => !string.IsNullOrEmpty(f));

        return data
            .Where(row => keyFields.All(f => row.ContainsKey(f) && row[f] != null))
            .ToList();
    }

    private static List<Dictionary<string, object>> Dedup(
        List<Dictionary<string, object>> data, DatasetConfig config)
    {
        var seen = new HashSet<string>();
        var result = new List<Dictionary<string, object>>();

        foreach (var row in data)
        {
            var key = JsonSerializer.Serialize(row);
            if (seen.Add(key))
                result.Add(row);
        }

        return result;
    }

    private static List<Dictionary<string, object>> Standardize(
        List<Dictionary<string, object>> data)
    {
        foreach (var row in data)
        {
            var keys = row.Keys.ToList();
            foreach (var key in keys)
            {
                if (row[key] is string s)
                    row[key] = s.Trim();
            }
        }

        return data;
    }

    private static List<Dictionary<string, object>> QualityFilter(
        List<Dictionary<string, object>> data, double minScore)
    {
        return data
            .Where(row => CalculateQualityScore(row) >= minScore)
            .ToList();
    }

    private static double CalculateQualityScore(Dictionary<string, object> row)
    {
        double score = 1.0;
        foreach (var value in row.Values)
        {
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    score -= 0.3;
                else if (s.Length < 2)
                    score -= 0.2;
                else if (s.Length > 10000)
                    score -= 0.1;
            }
        }
        return Math.Max(0, score);
    }
}
