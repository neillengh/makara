using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Makara.Desktop.Services;

/// <summary>
/// 本地运行历史（仪表盘"最近执行记录"与"近 7 天运行趋势"数据来源）
/// </summary>
public class RunRecord
{
    public string RunId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    /// <summary>running / success / failed / cancelled</summary>
    public string Status { get; set; } = "running";
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime EndTime { get; set; } = DateTime.MinValue;

    public double DurationSeconds =>
        EndTime == DateTime.MinValue
            ? (DateTime.Now - StartTime).TotalSeconds
            : (EndTime - StartTime).TotalSeconds;

    public string DurationText => DurationSeconds >= 60
        ? $"{(int)DurationSeconds / 60}m {(int)DurationSeconds % 60:00}s"
        : $"{(int)DurationSeconds:0}s";
}

public class RunHistoryService
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Makara");
    private static readonly string FilePath = Path.Combine(Dir, "run-history.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>最近记录在前</summary>
    public ObservableCollection<RunRecord> Records { get; } = [];

    public RunHistoryService()
    {
        Load();
    }

    public RunRecord StartRun(string runId, string workflowName)
    {
        var record = new RunRecord
        {
            RunId = runId,
            WorkflowName = workflowName,
            Status = "running",
            StartTime = DateTime.Now
        };
        Records.Insert(0, record);
        while (Records.Count > 100) Records.RemoveAt(Records.Count - 1);
        Save();
        return record;
    }

    public void FinishRun(string runId, string status)
    {
        var record = Records.FirstOrDefault(r => r.RunId == runId);
        if (record is null) return;
        record.Status = status;
        record.EndTime = DateTime.Now;
        Save();
    }

    /// <summary>近 N 天每天运行次数（含成功与失败，不含运行中）</summary>
    public int[] DailyCounts(int days = 7)
    {
        var today = DateTime.Today;
        var counts = new int[days];
        foreach (var r in Records)
        {
            if (r.Status == "running") continue;
            var diff = (today - r.StartTime.Date).Days;
            if (diff >= 0 && diff < days) counts[days - 1 - diff]++;
        }
        return counts;
    }

    public int SuccessCount => Records.Count(r => r.Status == "success");

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Records.ToList(), JsonOpts));
        }
        catch { /* 持久化失败不中断应用 */ }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var list = JsonSerializer.Deserialize<List<RunRecord>>(File.ReadAllText(FilePath));
            if (list is null) return;
            foreach (var r in list.OrderByDescending(r => r.StartTime)) Records.Add(r);
        }
        catch { /* 忽略损坏的历史 */ }
    }
}
