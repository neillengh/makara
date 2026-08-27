using CommunityToolkit.Mvvm.ComponentModel;
using Makara.Core.Models;

namespace Makara.Desktop.ViewModels;

public partial class NodeItemViewModel : ObservableObject
{
    public const double NodeWidth = 180;
    public const double NodeHeight = 56;

    private readonly WorkflowNode _node;

    public string Id => _node.Id;
    public string Type => _node.Type;
    public string Category => GetCategory(_node.Type);
    public string Icon => GetIcon(_node.Type);

    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isConnecting;

    public double InputPortX => X;
    public double InputPortY => Y + NodeHeight / 2;
    public double OutputPortX => X + NodeWidth;
    public double OutputPortY => Y + NodeHeight / 2;

    public WorkflowNode ToModel() => _node;

    public NodeItemViewModel(WorkflowNode node)
    {
        _node = node;
        _label = string.IsNullOrWhiteSpace(node.Label) ? GetDefaultLabel(node.Type) : node.Label;
        _x = node.X;
        _y = node.Y;
    }

    public void SyncBack()
    {
        _node.Label = Label;
        _node.X = X;
        _node.Y = Y;
    }

    partial void OnXChanged(double value)
    {
        _node.X = value;
        OnPropertyChanged(nameof(InputPortX));
        OnPropertyChanged(nameof(OutputPortX));
    }

    partial void OnYChanged(double value)
    {
        _node.Y = value;
        OnPropertyChanged(nameof(InputPortY));
        OnPropertyChanged(nameof(OutputPortY));
    }

    public static string GetDefaultLabel(string type) => type switch
    {
        "trigger" => "触发器",
        "datasource" => "数据源",
        "dataclean" => "数据清洗",
        "fieldmap" => "字段映射",
        "qualitycheck" => "质量检查",
        "datasetbuild" => "数据集构建",
        "finetune" => "模型微调",
        "evaluate" => "模型评估",
        "deploy" => "模型部署",
        "notify" => "通知",
        "condition" => "条件分支",
        _ => type
    };

    public static string GetCategory(string type) => type switch
    {
        "trigger" => "触发器",
        "datasource" or "dataclean" or "fieldmap" or "qualitycheck" or "datasetbuild" => "数据",
        "finetune" or "evaluate" or "deploy" => "模型",
        _ => "其他"
    };

    public static string GetIcon(string type) => type switch
    {
        "trigger" => "⚡",
        "datasource" => "🔗",
        "dataclean" => "🧹",
        "fieldmap" => "📋",
        "qualitycheck" => "✓",
        "datasetbuild" => "📦",
        "finetune" => "🔥",
        "evaluate" => "📊",
        "deploy" => "🚀",
        "notify" => "🔔",
        "condition" => "🔀",
        _ => "❓"
    };
}
