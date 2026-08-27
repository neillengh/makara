using System.Text.Json;
using Makara.Core.Enums;
using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Infrastructure.DataProcessing;

public class DatasetBuilder : IDatasetBuilder
{
    public async Task<DatasetResult> BuildAsync(
        IEnumerable<Dictionary<string, object>> rawData,
        DatasetConfig config,
        string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var records = rawData.ToList();
        var (trainRecords, valRecords) = SplitTrainVal(records, config.ValSplit);

        var trainPath = Path.Combine(outputDir, $"{config.Name}_train.jsonl");
        var valPath = Path.Combine(outputDir, $"{config.Name}_val.jsonl");

        await WriteAsync(trainPath, trainRecords, config);
        await WriteAsync(valPath, valRecords, config);

        return new DatasetResult
        {
            TrainPath = trainPath,
            ValPath = valPath,
            TotalCount = records.Count,
            TrainCount = trainRecords.Count,
            ValCount = valRecords.Count
        };
    }

    public string ConvertRecord(
        Dictionary<string, object> row,
        DatasetConfig config)
    {
        return config.OutputFormat switch
        {
            DatasetFormat.QA => ConvertQA(row, config.Mapping),
            DatasetFormat.Instruction => ConvertInstruction(row, config.Mapping),
            DatasetFormat.PlainText => ConvertPlainText(row, config.Mapping),
            DatasetFormat.MultiTurn => ConvertMultiTurn(row, config.Mapping),
            _ => ConvertQA(row, config.Mapping)
        };
    }

    private static string ConvertQA(Dictionary<string, object> row, FieldMapping mapping)
    {
        var question = row.GetValueOrDefault(mapping.QuestionField)?.ToString() ?? "";
        var answer = row.GetValueOrDefault(mapping.AnswerField)?.ToString() ?? "";

        var entry = new
        {
            conversations = new[]
            {
                new { role = "system", content = mapping.SystemPrompt ?? "" },
                new { role = "user", content = question },
                new { role = "assistant", content = answer }
            }
        };
        return JsonSerializer.Serialize(entry);
    }

    private static string ConvertInstruction(Dictionary<string, object> row, FieldMapping mapping)
    {
        var instruction = row.GetValueOrDefault(mapping.QuestionField)?.ToString() ?? "";
        var input = row.GetValueOrDefault(mapping.InputField ?? "")?.ToString() ?? "";
        var output = row.GetValueOrDefault(mapping.AnswerField)?.ToString() ?? "";

        var entry = new
        {
            instruction,
            input,
            output
        };
        return JsonSerializer.Serialize(entry);
    }

    private static string ConvertPlainText(Dictionary<string, object> row, FieldMapping mapping)
    {
        var text = row.GetValueOrDefault(mapping.QuestionField)?.ToString() ?? "";
        if (!string.IsNullOrEmpty(mapping.AnswerField))
        {
            var answer = row.GetValueOrDefault(mapping.AnswerField)?.ToString() ?? "";
            text = string.Join("\n", text, answer);
        }
        return JsonSerializer.Serialize(new { text });
    }

    private static string ConvertMultiTurn(Dictionary<string, object> row, FieldMapping mapping)
    {
        var question = row.GetValueOrDefault(mapping.QuestionField)?.ToString() ?? "";
        var answer = row.GetValueOrDefault(mapping.AnswerField)?.ToString() ?? "";

        var turns = question.Split("|||", StringSplitOptions.RemoveEmptyEntries);

        var conversations = new List<object>();
        if (!string.IsNullOrEmpty(mapping.SystemPrompt))
            conversations.Add(new { role = "system", content = mapping.SystemPrompt });

        for (var i = 0; i < turns.Length; i++)
        {
            conversations.Add(new { role = "user", content = turns[i].Trim() });
        }
        conversations.Add(new { role = "assistant", content = answer });

        return JsonSerializer.Serialize(new { conversations });
    }

    private static (List<T> train, List<T> val) SplitTrainVal<T>(
        List<T> data, double valSplit)
    {
        if (valSplit <= 0 || valSplit >= 1)
            return (data, []);

        var valCount = (int)(data.Count * valSplit);
        var shuffled = data.OrderBy(_ => Random.Shared.Next()).ToList();

        return (shuffled[..^valCount], shuffled[^valCount..]);
    }

    private static async Task WriteAsync(
        string path,
        List<Dictionary<string, object>> records,
        DatasetConfig config)
    {
        await using var writer = new StreamWriter(path);
        var builder = new DatasetBuilder();
        foreach (var record in records)
        {
            var line = record.Count > 0
                ? builder.ConvertRecord(record, config)
                : "";
            await writer.WriteLineAsync(line);
        }
    }
}
