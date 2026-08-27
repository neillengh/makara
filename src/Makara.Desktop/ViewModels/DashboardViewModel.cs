using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Core.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

/// <summary>
/// 仪表盘 ViewModel：KPI 统计、近 7 天运行趋势、最近执行记录、最近工作流/数据集
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private const int ChartW = 700;
    private const int ChartH = 200;
    private const int Padding = 36;

    private readonly ApiClient _api;
    private readonly RunHistoryService _runHistory;

    // === KPI ===
    [ObservableProperty] private int _kpiWorkflows;
    [ObservableProperty] private int _kpiDatasets;
    [ObservableProperty] private int _kpiSuccess;
    [ObservableProperty] private string _serverStatus = "检测中...";

    // === 趋势图 ===
    [ObservableProperty] private PointCollection _trendPoints = [];
    [ObservableProperty] private PointCollection _trendFill = [];
    [ObservableProperty] private ObservableCollection<string> _trendLabels = [];
    [ObservableProperty] private double _trendMax = 10;
    [ObservableProperty] private ObservableCollection<ChartPoint> _chartPoints = [];

    // === 列表 ===
    [ObservableProperty] private ObservableCollection<RunRecord> _recentRuns = [];
    [ObservableProperty] private ObservableCollection<Workflow> _recentWorkflows = [];
    [ObservableProperty] private ObservableCollection<DatasetInfo> _recentDatasets = [];

    [ObservableProperty] private string _statusMessage = string.Empty;

    public DashboardViewModel(ApiClient api, RunHistoryService runHistory)
    {
        _api = api;
        _runHistory = runHistory;
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;

        // 本地运行历史
        KpiSuccess = _runHistory.SuccessCount;
        RecentRuns = new ObservableCollection<RunRecord>(_runHistory.Records.Take(5));
        BuildTrend(_runHistory.DailyCounts(7));

        try
        {
            var workflows = await _api.GetWorkflowsAsync() ?? [];
            KpiWorkflows = workflows.Count;
            RecentWorkflows = new ObservableCollection<Workflow>(
                workflows.OrderByDescending(w => w.UpdatedAt).Take(3));
        }
        catch (Exception ex)
        {
            StatusMessage = $"工作流加载失败: {ex.Message}";
        }

        try
        {
            var datasets = await _api.GetDatasetsAsync() ?? [];
            KpiDatasets = datasets.Count;
            RecentDatasets = new ObservableCollection<DatasetInfo>(
                datasets.OrderByDescending(d => d.UpdatedAt).Take(3));
        }
        catch (Exception ex)
        {
            StatusMessage = StatusMessage == string.Empty
                ? $"数据集加载失败: {ex.Message}"
                : StatusMessage + $" | 数据集加载失败: {ex.Message}";
        }

        var online = await _api.CheckHealthAsync();
        ServerStatus = online ? "在线" : "离线";
    }

    /// <summary>根据近 7 天每日运行次数计算折线坐标（700x200 视区）</summary>
    private void BuildTrend(int[] dailyCounts)
    {
        TrendLabels = new ObservableCollection<string>(
            Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(i - 6).ToString("MM-dd")));

        TrendMax = Math.Max(10, dailyCounts.Max() * 1.2);
        var step = (ChartW - Padding * 2) / 6.0;
        var height = ChartH - 24.0;

        var points = new PointCollection();
        var chartPoints = new ObservableCollection<ChartPoint>();
        for (var i = 0; i < 7; i++)
        {
            var x = Padding + step * i;
            var y = 12 + height * (1 - dailyCounts[i] / TrendMax);
            points.Add(new Point(x, y));
            chartPoints.Add(new ChartPoint
            {
                X = x - 4,
                Y = y - 4,
                Tooltip = $"{TrendLabels[i]} · {dailyCounts[i]} 次"
            });
        }
        TrendPoints = points;
        ChartPoints = chartPoints;

        var fill = new PointCollection(points)
        {
            new Point(ChartW - Padding, ChartH - 12),
            new Point(Padding, ChartH - 12)
        };
        TrendFill = fill;
    }

    public static string SampleCountText(DatasetInfo d) =>
        d.SampleCount >= 10000 ? $"{d.SampleCount / 10000.0:0.#} 万条" : $"{d.SampleCount:N0} 条";
}

/// <summary>趋势图数据点</summary>
public class ChartPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Tooltip { get; set; } = string.Empty;
}
