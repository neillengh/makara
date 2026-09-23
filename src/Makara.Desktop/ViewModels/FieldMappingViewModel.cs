using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Makara.Desktop.ViewModels;

public class SourceField
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}

/// <summary>
/// 字段映射页：源字段 → 训练格式目标字段，实时生成 JSON 样本预览
/// </summary>
public partial class FieldMappingViewModel : ObservableObject
{
    public ObservableCollection<SourceField> SourceFields { get; }

    /// <summary>目标字段下拉选项：-- 不映射 -- + 各源字段名</summary>
    public List<string> SourceOptions { get; }

    public List<string> TargetFields { get; } =
        new() { "instruction", "input", "output", "history", "system", "__index__" };

    [ObservableProperty] private string _mapInstruction = "notes";
    [ObservableProperty] private string _mapInput = "region";
    [ObservableProperty] private string _mapOutput = "amount";
    [ObservableProperty] private string _mapHistory = "-- 不映射 --";
    [ObservableProperty] private string _mapSystem = "product_name";
    [ObservableProperty] private string _mapIndex = "customer_id";

    [ObservableProperty] private string _previewJson = "";
    [ObservableProperty] private string _statusMessage = "";

    private static readonly Dictionary<string, object?> SampleValues = new()
    {
        ["customer_id"] = 10042,
        ["order_date"] = "2026-01-15",
        ["amount"] = 1299.50,
        ["region"] = "North America",
        ["product_name"] = "Premium Wireless Headphones",
        ["notes"] = "查看订单备注",
    };

    public FieldMappingViewModel()
    {
        SourceFields = new ObservableCollection<SourceField>
        {
            new() { Name = "customer_id", Type = "int" },
            new() { Name = "order_date", Type = "date" },
            new() { Name = "amount", Type = "float" },
            new() { Name = "region", Type = "string" },
            new() { Name = "product_name", Type = "string" },
            new() { Name = "notes", Type = "text" },
        };
        SourceOptions = new List<string> { "-- 不映射 --" };
        foreach (var f in SourceFields) SourceOptions.Add(f.Name);

        RebuildPreview();
    }

    // 任一映射变更后重建预览
    partial void OnMapInstructionChanged(string value) => RebuildPreview();
    partial void OnMapInputChanged(string value) => RebuildPreview();
    partial void OnMapOutputChanged(string value) => RebuildPreview();
    partial void OnMapHistoryChanged(string value) => RebuildPreview();
    partial void OnMapSystemChanged(string value) => RebuildPreview();
    partial void OnMapIndexChanged(string value) => RebuildPreview();

    private void RebuildPreview()
    {
        object? Val(string map) => map == "-- 不映射 --" ? null : SampleValues.GetValueOrDefault(map, map);
        var obj = new Dictionary<string, object?>
        {
            ["instruction"] = Val(MapInstruction),
            ["input"] = Val(MapInput),
            ["output"] = Val(MapOutput),
            ["history"] = Val(MapHistory),
            ["system"] = Val(MapSystem),
            ["__index__"] = Val(MapIndex),
        };
        PreviewJson = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    [RelayCommand]
    private void SaveMapping() => StatusMessage = "字段映射已保存（演示）";

    [RelayCommand]
    private void Preview() => StatusMessage = "已刷新样本预览";

    [RelayCommand]
    private void Prev() => StatusMessage = "已是第一步：选择数据源";

    [RelayCommand]
    private void Next() => StatusMessage = "即将进入：数据集生成（演示）";
}
