using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Core.Enums;
using Makara.Core.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

/// <summary>
/// 数据集行 ViewModel（列表展示用）
/// </summary>
public partial class DatasetRowViewModel : ObservableObject
{
    public DatasetInfo Model { get; }

    public DatasetRowViewModel(DatasetInfo model) => Model = model;

    public string Name => Model.Name;
    public string FormatText => Model.ContentFormat switch
    {
        DatasetFormat.QA => "QA",
        DatasetFormat.Instruction => "Instruction",
        DatasetFormat.PlainText => "PlainText",
        DatasetFormat.MultiTurn => "MultiTurn",
        _ => Model.ContentFormat.ToString()
    };
    public long SampleCount => Model.SampleCount;
    public string CreatedAtText => Model.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd");
    public double QualityScore => Model.QualityScore;

    public string QualityColor => Model.QualityScore >= 90 ? "Success" : Model.QualityScore >= 60 ? "Warning" : "Error";

    public string StatusText => Model.Status switch
    {
        DatasetStatus.Building => "处理中",
        DatasetStatus.Ready => "可用",
        DatasetStatus.Failed => "失败",
        _ => Model.Status.ToString()
    };

    /// <summary>状态画刷（按当前主题解析）</summary>
    public System.Windows.Media.Brush StatusBrush =>
        ResolveBrush(Model.Status switch
        {
            DatasetStatus.Building => "MkInfo",
            DatasetStatus.Ready => "MkSuccess",
            DatasetStatus.Failed => "MkError",
            _ => "MkTextSecondary"
        });

    /// <summary>质量分画刷</summary>
    public System.Windows.Media.Brush QualityBrush => ResolveBrush(QualityColor);

    private static System.Windows.Media.Brush ResolveBrush(string key) =>
        (System.Windows.Media.Brush)(System.Windows.Application.Current.TryFindResource(key)
            ?? System.Windows.Media.Brushes.Gray);
}

/// <summary>
/// 样本预览条目：把 JSON 字符串解析为字段展示
/// </summary>
public class SampleFieldViewModel
{
    public string Field { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsInput { get; init; }
}

/// <summary>
/// 数据集管理页 ViewModel
/// </summary>
public partial class DatasetsViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private ObservableCollection<DatasetRowViewModel> _datasets = [];
    [ObservableProperty] private DatasetRowViewModel? _selectedDataset;
    [ObservableProperty] private ObservableCollection<SampleFieldViewModel> _sampleFields = [];
    [ObservableProperty] private ObservableCollection<SampleGroupViewModel> _samples = [];
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    // 统计卡
    [ObservableProperty] private int _totalDatasets;
    [ObservableProperty] private long _totalSamples;
    [ObservableProperty] private double _avgQuality;
    [ObservableProperty] private int _todayNew;

    public DatasetsViewModel(ApiClient api)
    {
        _api = api;
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        try
        {
            var list = await _api.GetDatasetsAsync() ?? [];
            Datasets = new ObservableCollection<DatasetRowViewModel>(list.Select(d => new DatasetRowViewModel(d)));

            TotalDatasets = Datasets.Count;
            TotalSamples = Datasets.Sum(d => d.SampleCount);
            AvgQuality = Datasets.Count > 0 ? Math.Round(Datasets.Average(d => d.QualityScore), 1) : 0;
            TodayNew = Datasets.Count(d => d.Model.CreatedAt.ToLocalTime().Date == DateTime.Today);

            SelectedDataset = Datasets.FirstOrDefault();
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedDatasetChanged(DatasetRowViewModel? value)
    {
        if (value is null)
        {
            Samples.Clear();
            return;
        }
        _ = LoadSamplesAsync(value);
    }

    private async Task LoadSamplesAsync(DatasetRowViewModel dataset)
    {
        Samples.Clear();
        try
        {
            var samples = await _api.GetDatasetSamplesAsync(dataset.Model.Id, 0, 3) ?? [];
            foreach (var s in samples)
            {
                var group = new SampleGroupViewModel { Index = s.RecordIndex };
                try
                {
                    using var doc = JsonDocument.Parse(s.JsonData);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var text = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                            JsonValueKind.Null or JsonValueKind.Undefined => "（无）",
                            _ => prop.Value.ToString()
                        };
                        group.Fields.Add(new SampleFieldViewModel
                        {
                            Field = prop.Name,
                            Value = text,
                            IsInput = prop.Name.Equals("input", StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
                catch
                {
                    group.Fields.Add(new SampleFieldViewModel { Field = "raw", Value = s.JsonData });
                }
                Samples.Add(group);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"样本加载失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(DatasetRowViewModel row)
    {
        try
        {
            if (await _api.DeleteDatasetAsync(row.Model.Id))
            {
                Datasets.Remove(row);
                StatusMessage = $"已删除数据集 {row.Name}";
                TotalDatasets = Datasets.Count;
            }
            else
            {
                StatusMessage = "删除失败：数据集不存在";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }
}

/// <summary>一组样本（预览面板中的一条样本）</summary>
public class SampleGroupViewModel
{
    public int Index { get; init; }
    public ObservableCollection<SampleFieldViewModel> Fields { get; } = [];
}
